using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using CorPolicy;
using Helpers;
using RulesLib.PolicySetup;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace RulesLib.Rules
{
    public class Rules2 : IPPARule
    {

        public bool CheckNEI(CorPolicy.clsPolicyPPA pol, Helpers.StateInfoHelper stateInfo, string connectionString, bool includeSymbol = false)
        {
            //NotesHelper.AddNote(pol, "Testing", "Testing NEI: Testing", "NEI");
            NotesHelper.RemoveNotes(pol.Notes, "NEI");
            string missingInfo = "";

            try
            {

                if (pol.EffDate == DateTime.MinValue)
                {                    
                    missingInfo += "EffDate-";
                }

                if (pol.DriverCount(true) < 1)
                {
                    missingInfo += "Drivers-";
                }

                if (VehicleHelper.VehicleCount(pol) < 1)
                {
                    missingInfo += "Vehicles-";
                }

                foreach (clsVehicleUnit veh in pol.VehicleUnits)
                {
                    if (!veh.IsMarkedForDelete)
                    {
                        if (veh.Zip == "" && veh.VinNo != "NONOWNER")
                        {
                            missingInfo += "Zip:Veh " + veh.IndexNum + "-";
                        }

                        if (veh.VehicleYear == "")
                        {
                            missingInfo += "VehicleYear:Veh " + veh.IndexNum + "-";
                        }                        
                        
                        if (includeSymbol)
                        {
                            if (veh.LiabilitySymbolCode == "")
                            {
                                missingInfo += "LiabilitySymbolCode:Veh " + veh.IndexNum + "-";
                            }

                            if (veh.PIPMedLiabilityCode == "")
                            {
                                missingInfo += "PIPMedLiabilityCode:Veh " + veh.IndexNum + "-";
                            }

                            if (veh.VehicleSymbolCode == "" && (veh.CollSymbolCode == "" && veh.CompSymbolCode == ""))
                            {
                                missingInfo += "VehicleSymbolCode:Veh " + veh.IndexNum + "-";
                            }

                            if (((veh.VehicleSymbolCode == "66" || veh.VehicleSymbolCode == "67" || veh.VehicleSymbolCode == "68") && Int32.Parse(veh.VehicleYear) < 2011) ||
                                    ((veh.VehicleSymbolCode == "966" || veh.VehicleSymbolCode == "967" || veh.VehicleSymbolCode == "968") && Int32.Parse(veh.VehicleYear) >= 2011))
                            {
                                if (veh.VehicleSymbolCode == "67" && Int32.Parse(veh.VehicleYear) < 2011 && pol.Program.ToUpper() == "SUMMIT" && (pol.StateCode == "03" || pol.StateCode == "42"))
                                {
                                    //Do nothing, AR SUMMIT used 67 as the invalid vin symbol, does not allow stated value
                                }
                                else
                                {
                                    if (veh.StatedAmt < 500 || veh.StatedAmt > 100000)
                                    {
                                        missingInfo += "InvalidStatedValueAmount:Veh " + veh.IndexNum + "-";
                                    }
                                }
                            }

                        }
                        else
                        {
                            if (veh.VinNo == string.Empty)
                            {
                                if (veh.VehicleMakeCode == string.Empty)
                                {
                                    missingInfo += "VehicleMakeCode:Veh" + veh.IndexNum + "-";
                                }
                                if (veh.VehicleModelCode == string.Empty)
                                {
                                    missingInfo += "VehicleModelCode:Veh" + veh.IndexNum + "-";
                                }
                                if (veh.VehicleBodyStyleCode == string.Empty)
                                {
                                    missingInfo += "VehicleBodyStyleCode:Veh" + veh.IndexNum + "-";
                                }
                            }
                            if (veh.StatedAmt != 0)
                            {
                                if (veh.StatedAmt < 500 || veh.StatedAmt > 100000)
                                {
                                    missingInfo += "InvalidStatedValueAmount:Veh " + veh.IndexNum + "-";
                                }
                            }
                        }
                        
                        if (veh.Coverages.Count < 1)
                        {
                            missingInfo += "Coverages:Veh " + veh.IndexNum + "-";
                        }

                    }
                }

                if (pol.PolicyInsured != null)
                {
                    clsEntityPolicyInsured ins = pol.PolicyInsured;
                    if (ins.MaritalStatus == "")
                    {
                        missingInfo += "MaritalStatus-";
                    }

                    if (ins.Age < 10)
                    {
                        missingInfo += "Age-";
                    }

                    if (ins.PriorLimitsCode == "")
                    {
                        missingInfo += "PriorLimitsCode-";
                    }
                }
                else
                {
                    missingInfo += "PolicyInsured-";
                }

                if (pol.PayPlanCode == "")
                {
                    missingInfo += "PayPlanCode-";
                }

                if (pol.CallingSystem != "PAS" && pol.CallingSystem != "AOLE" && pol.CallingSystem != "UWOLE")
                {
                    if (pol.LienHolders.Count > 0)
                    {
                        foreach (clsEntityLienHolder lien in pol.LienHolders)
                        {
                            if (lien.EntityType == "PFC")
                            {
                                if (pol.PayPlanCode != "100")
                                {
                                    missingInfo += "PremiumFinanceCompany-";
                                }
                                break;
                            }
                        }
                    }
                }

                if (!DriverHelper.HasActiveDrivers(pol)) 
                {
                    missingInfo += "NoActiveDrivers-";
                }

                if (missingInfo == "")
                {
                    return true;
                }
                else
                {
                    NotesHelper.AddNote(pol, "Needs: " + missingInfo, "Not Enough Information To Rate", "NEI");
                    return false;
                }

            }
            catch(Exception ex)
            {
                NotesHelper.AddNote(pol, ex.Message + " Needs: " + missingInfo, "Not Enough Information To Rate", "NEI");
                return false;
            }
        }        

        private DataTable FindRules(clsPolicyPPA pol, string ruleType, string ruleSubType, string status, string connectionString)
        {
            string SQL = "";
            int numStatus = 0;
            if (!Int32.TryParse(status, out numStatus))
            {
                numStatus = 4;
            }            

            SQL = " SELECT FunctionName, OrderNumber ";
            //SQL += " FROM pgm" + pol.Product + pol.StateCode + ".." + "CodeCreditTiers with(nolock)";
            SQL += " FROM Common..RatingRules with(nolock) ";
            SQL += " WHERE Product = @Product ";
            SQL += " AND EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND CallingSystem IN ('ALL',  @CallingSystem ) ";
            SQL += " AND RuleType = @RuleType ";
            SQL += " AND State in ('ALL', @StateCode) ";
            SQL += " AND Program in ('ALL', @Program) ";
            SQL += " AND SubType = @SubType ";
            SQL += " AND Status <= @Status ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@Product", SqlDbType.Int, 22, pol.Product));
            parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
            parms.Add(DBHelper.AddParm("@RuleType", SqlDbType.VarChar, 11, ruleType));
            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@CallingSystem", SqlDbType.VarChar, 11, PolicyHelper.NormalizeCallingSystem(pol.CallingSystem)));
            parms.Add(DBHelper.AddParm("@Status", SqlDbType.Int, 22, numStatus));
            parms.Add(DBHelper.AddParm("@StateCode", SqlDbType.VarChar, 11, pol.StateCode));
            parms.Add(DBHelper.AddParm("@SubType", SqlDbType.VarChar, 11, ruleSubType));

            DataTable commonRules = DBHelper.GetDataTable(SQL, "CommonRules", connectionString, parms, "common");

            SQL = " SELECT FunctionName, OrderNumber ";
            SQL += " FROM pgm" + pol.Product + pol.StateCode + ".." + "RatingRules with(nolock)";            
            SQL += " WHERE EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND CallingSystem IN ('ALL',  @CallingSystem ) ";
            SQL += " AND RuleType = @RuleType ";
            SQL += " AND State in ('ALL', @StateCode) ";
            SQL += " AND Program in ('ALL', @Program) ";
            SQL += " AND SubType = @SubType ";
            SQL += " AND Status <= @Status ";

            DataTable stateRules = DBHelper.GetDataTable(SQL, "CommonRules", connectionString, parms);

            DataTable fullRules = stateRules.Clone();
            foreach (DataRow row in commonRules.Rows)
            {
                fullRules.ImportRow(row);
            }
            foreach (DataRow row in stateRules.Rows)
            {
                fullRules.ImportRow(row);
            }
            if(fullRules.Rows.Count > 0)
                return fullRules.Select("", "OrderNumber ASC").CopyToDataTable();
            return fullRules;
        }

        private DataTable ApplyTemporaryRulesOverride(DataTable rules, clsPolicyPPA pol){
            if (pol.CallingSystem.ToUpper().Trim() != "RENEWAL_SVC" && (rules.Rows.OfType<DataRow>().Any(a => a["FunctionName"].ToString() == "CheckNonOwnerNotAllowed")))
            {
                List<DataRow> rows = rules.Rows.OfType<DataRow>().Where(a => a["FunctionName"].ToString() == "CheckNonOwner").ToList();
                foreach (DataRow row in rows)
                {
                    rules.Rows.Remove(row);
                }
            }
            return rules;
        }

        public bool CheckIER(string ruleLevel, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            NotesHelper.RemoveNotes(pol.Notes, "IER");
            if (pol.PolicyID.Length == 0 && pol.AppliesToCode == "N")
            {
                DriverHelper.CheckForDriverRestrictions(pol, connectionString);
            }
            return CheckRules("IER", ruleLevel, pol, stateInfo, connectionString);
        }

        public bool CheckWRN(string ruleLevel, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            NotesHelper.RemoveNotes(pol.Notes, "WRN");
            return CheckRules("WRN", ruleLevel, pol, stateInfo, connectionString);
        }

        public bool CheckUWW(string ruleLevel, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            NotesHelper.RemoveNotes(pol.Notes, "UWW");
            return CheckRules("UWW", ruleLevel, pol, stateInfo, connectionString);
        }

        public bool CheckRES(string ruleLevel, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            NotesHelper.RemoveNotes(pol.Notes, "RES");
            return CheckRules("RES", ruleLevel, pol, stateInfo, connectionString);
        }

        private bool CheckRules(string ruleType, string ruleLevel, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            
            DataTable rules = null;
            if (pol.Status.ToUpper().Trim() != "BOUND")
            {
                rules = FindRules(pol, ruleType, ruleLevel, pol.Status, connectionString);
                rules = ApplyTemporaryRulesOverride(rules, pol);
                Type t = Type.GetType("RulesLib.Rules.StateRules" + pol.Product + pol.StateCode);

                foreach (DataRow rule in rules.Rows)
                {
                    string functionName = rule["FunctionName"].ToString();
                    try
                    {

                        MethodInfo mi = t.GetMethod(functionName);
                        if (mi != null)
                        {
                            IPPAStateRule r = (IPPAStateRule)Activator.CreateInstance(t);
                            Object[] args = { pol, stateInfo, connectionString };
                            mi.Invoke(r, args);
                        }
                    }
                    catch
                    {
                        //Do Nothing for now;
                    }
                }
                t = null;
            }

            return true;
        }

        public void SetupPolicyData(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {
            VehicleSymbolSetup.Execute(ref pol, connectionString, stateInfo);
            VehicleDriverAssignmentSetup.Execute(ref pol, connectionString, stateInfo);
            CreditTierSetup.Execute(ref pol, connectionString, stateInfo);
            PriorLimitsSetup.Execute(ref pol, connectionString, stateInfo);
            DaysLapseSetup.Execute(ref pol, connectionString, stateInfo);
            MonthsPriorContCovSetup.Execute(ref pol, connectionString, stateInfo);
            UWTierSetup.Execute(ref pol, connectionString, stateInfo);
            DriverSR22Setup.Execute(ref pol, connectionString, stateInfo);
            
            VehiclesTrueAgeSetup.Execute(ref pol, connectionString, stateInfo);
            ViolationPointsSetup.Execute(ref pol, connectionString, stateInfo);
            NoteSetup.Execute(ref pol, connectionString, stateInfo);            
            FactorDriverAutoApplySetup.Execute(ref pol, connectionString, stateInfo);
            FactorVehicleAutoApplySetup.Execute(ref pol, connectionString, stateInfo);
            FactorPolicyAutoApplySetup.Execute(ref pol, connectionString, stateInfo);
            ViolationPointsSetup.Execute(ref pol, connectionString, stateInfo);            
        }

        public void Finish(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {
            VehiclesAgeSetup.Execute(ref pol, connectionString, stateInfo);
        }
    }
}
