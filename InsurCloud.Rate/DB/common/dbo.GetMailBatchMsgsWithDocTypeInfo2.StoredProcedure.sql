USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[GetMailBatchMsgsWithDocTypeInfo2]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

























/**
**/
-- ====================================================
-- Author:        
-- Create date: 
-- Description:   
-- ====================================================
CREATE PROCEDURE [dbo].[GetMailBatchMsgsWithDocTypeInfo2]
      -- Add the parameters for the stored procedure here
      (@Product varchar(1),@StateCode varchar(2), @MsgEffDate Datetime)
AS

BEGIN
      -- SET NOCOUNT ON added to prevent extra result sets from
      -- interfering with SELECT statements.
      SET NOCOUNT ON;

----  FOR TESTING PURPOSES
--    Declare @Product varchar(1), @StateCode varchar(2), @MsgEffDate Datetime
--    set @Product='1'
--    set @StateCode='42'
--    set @MsgEffDate='4/17/2011'
----  END FOR TESTING PURPOSES
      
      Declare @cmd varchar(max)
      set @cmd =  'Select MsgID, MsgEffDate, MsgType, MsgSubType, a.PolicyID, a.TermEffDate, a.PolicyTransactionNum,a.Product,a.State, IsNull(Param1,'''') as Param1, IsNull(Param2,'''') as Param2, IsNull(a.Param3,'''') as Param3, IsNull(ParamXML,'''') as ParamXML, ' +
                        '     d.EditCode as SequenceNum, b.FilePath, DocType,IsNull(c.State,''XX'') as State, IsNull(Stock,''XX'') as Stock, grping.Stapling as Stapling , grping.BRE as BRE, grping.Special,' +
                        '      grping.FooterAttachment as FooterAttachment, IsNull(c.CERTType,ParamXML.value(''(/clsParameterSet/CERTType)[1]'', ''VARCHAR(50)'')) as CERTType, IsNull(PlexType,''XX'') as PlexType ' +
						--' ,grping.Stapling as StaplingNew,grping.BRE as BRE_NEW, grping.FooterAttachment as FooterAttachment_NEW ' +
                        'FROM pgm' + @Product + @StateCode + '..BatchMsg a with (NOLOCK)' +
                        '     join pgm' + @Product + @StateCode + '..ImagingPolicy b with (NOLOCK) on a.Param1=b.ImageID ' +
                        '     join pgm' + @Product + @StateCode + '..DocType c with (NOLOCK) on b.DocumentType = c.DocType collate database_default ' +
                        ' AND c.EffDate <= a.TermEffDate AND c.ExpDate > a.TermEffDate ' +
                        '     join Imaging..EditCode d with (NOLOCK) on b.DocumentType = d.EditDescription AND d.EditGroup = ''SEQUENCE''' +

						' join (select  bm2.PolicyID,Param3,isNull(CERTType,''OTR'') as CERTTYpe, ' +
						' max(Stapling) as Stapling, ' +
						--' case Max(Case BRE when ''XX'' then '''' else BRE END) when '''' then ''XX'' else max(Case BRE when ''XX'' then '''' else BRE END) END as BRE, ' +
						' case Max(Case BRE when ''XX'' then '''' else BRE END) when '''' then ''XX'' else Max(Case BRE when ''XX'' then '''' else BRE END)  END as BRE, ' +
						' case Max(Case FooterAttachment when ''XX'' then '''' else FooterAttachment END ) when '''' then ''XX'' else Max(Case FooterAttachment when ''XX'' then '''' else FooterAttachment END ) END as FooterAttachment, ' +
						' case Max(Case Special when ''OTR'' then '''' else Special End) when '''' then ''OTR'' else Max(Case Special when ''OTR'' then '''' else Special End) end as Special ' +
						' from pgm' + @Product + @StateCode + '..batchmsg bm2 with (NOLOCK) ' +
						' join pgm' + @Product + @StateCode + '..imagingpolicy i with (NOLOCK) on bm2.Param1=i.ImageID ' +
						' join pgm' + @Product + @StateCode + '..doctype dt with (NOLOCK) on i.DocumentType=dt.DocType collate database_default ' +
						' and dt.EffDate<=bm2.TermEffDate and dt.ExpDate>bm2.TermEffDate ' +
						' join Imaging..EditCode iec with (NOLOCK) on i.DocumentType=iec.EditDescription and iec.EditGroup=''SEQUENCE'' ' +
						' where bm2.MsgType=''Mail'' and bm2.MsgEffDate <= ''' +  Convert(varchar,@MsgEffDate,101) + '''' +
						' and (dt.Program is null or dt.Program in (''ALL'',''PPA'',''HOM'') or ( dt.Program collate database_default = isNull(bm2.PolicyXML.value(''(/clsPolicyPPA/ProgramCode)[1]'', ''VARCHAR(50)'') ,bm2.PolicyXML.value(''(/clsPolicyHomeOwner/ProgramCode)[1]'', ''VARCHAR(50)'')))) ' +
						' group BY bm2.PolicyID,Param3,isNull(CERTType,''OTR'') ) grping ' +
						' on a.PolicyID=grping.PolicyID and a.Param3=grping.Param3 and IsNull(c.CERTType,ParamXML.value(''(/clsParameterSet/CERTType)[1]'', ''VARCHAR(50)''))=grping.CERTTYpe ' +

                        ' WHERE a.MsgType=''Mail'' and a.MsgEffDate <= ''' +  Convert(varchar,@MsgEffDate,101) + '''' +
                        ' and (c.Program is null or c.Program in (''ALL'',''PPA'',''HOM'') or ( c.Program collate database_default = isNull(a.PolicyXML.value(''(/clsPolicyPPA/ProgramCode)[1]'', ''VARCHAR(50)'') ,a.PolicyXML.value(''(/clsPolicyHomeOwner/ProgramCode)[1]'', ''VARCHAR(50)'')))) ' +
                        ' ORDER BY a.PolicyID,a.Param3,Isnull(c.CERTType, ParamXML.value(''(/clsParameterSet/CERTType)[1]'', ''VARCHAR(50)'')),grping.Stapling,d.EditCode'
--select @cmd
      EXEC (@cmd)
END 
SET QUOTED_IDENTIFIER OFF







GO
