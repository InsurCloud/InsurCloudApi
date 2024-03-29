USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[GetSR22MailBatchMsgsWithDocTypeInfo]    Script Date: 7/27/2014 2:06:56 PM ******/
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
CREATE PROCEDURE [dbo].[GetSR22MailBatchMsgsWithDocTypeInfo]
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
--    set @StateCode='17'
--    set @MsgEffDate='10/31/2010'
----  END FOR TESTING PURPOSES
      
      Declare @cmd varchar(3000)
      set @cmd =  'Select MsgID, MsgEffDate, MsgType, MsgSubType, a.PolicyID, a.TermEffDate, a.PolicyTransactionNum,a.Product,a.State, IsNull(Param1,'''') as Param1, IsNull(Param2,'''') as Param2, IsNull(Param3,'''') as Param3, IsNull(ParamXML,'''') as ParamXML, ' +
                        '     d.EditCode as SequenceNum, b.FilePath, DocType,IsNull(c.State,''XX'') as State, IsNull(Stock,''XX'') as Stock, IsNull(Stapling,ParamXML.value(''(/clsParameterSet/Stapling)[1]'', ''VARCHAR(50)''))as Stapling , IsNull(BRE,ParamXML.value(''(/clsParameterSet/BRE)[1]'', ''VARCHAR(50)''))as BRE, IsNull(Special, ParamXML.value(''(/clsParameterSet/Special)[1]'', ''VARCHAR(50)'') ) as Special,' +
                        '      IsNull(FooterAttachment,ParamXML.value(''(/clsParameterSet/FooterAttachment)[1]'', ''VARCHAR(50)'')) as FooterAttachment, IsNull(CERTType,ParamXML.value(''(/clsParameterSet/CERTType)[1]'', ''VARCHAR(50)'')) as CERTType, IsNull(PlexType,''XX'') as PlexType ' +
                        'FROM pgm' + @Product + @StateCode + '..BatchMsg a with (NOLOCK)' +
                        '     join pgm' + @Product + @StateCode + '..ImagingPolicy b with (NOLOCK) on a.Param1=b.ImageID ' +
                        '     join pgm' + @Product + @StateCode + '..DocType c with (NOLOCK) on b.DocumentType = c.DocType collate database_default ' +
                        ' AND c.EffDate <= a.TermEffDate AND c.ExpDate > a.TermEffDate ' +
                        '     join Imaging..EditCode d with (NOLOCK) on b.DocumentType = d.EditDescription AND d.EditGroup = ''SEQUENCE''' +
                        'WHERE a.MsgType=''MAIL'' and a.MsgEffDate <= ''' +  Convert(varchar,@MsgEffDate,101) + '''' +
                        ' and (c.Program is null or c.Program in (''ALL'',''PPA'',''HOM'') or ( c.Program collate database_default = isNull(a.PolicyXML.value(''(/clsPolicyPPA/ProgramCode)[1]'', ''VARCHAR(50)'') ,a.PolicyXML.value(''(/clsPolicyHomeOwner/ProgramCode)[1]'', ''VARCHAR(50)'')))) ' +
                        ' and a.MsgSubType in (''SR22'',''SR26'') ' +
						' ORDER BY PolicyID,Param3,CERTType,Stapling,d.EditCode'
--select @cmd
      EXEC (@cmd)
END 
SET QUOTED_IDENTIFIER OFF








GO
