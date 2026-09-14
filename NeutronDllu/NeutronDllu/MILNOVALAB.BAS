' 9/24/2015
' Changed Printer Timeout value from 10000 to 60000 (10 seconds to 60 seconds)

' *************************************************************************************
'         nova sku labels
'         ReqFun=1..sku label requires printing
'         ReqFun=2..REPRINT LABELS
'         ReqFun=3..PRINT A GIVEN NUMBER OF LABELS ON DEMAND FOR A POSITION
'                   SKUINDEX HAS THE NUMBER OF LABELS
' *************************************************************************************
    SUB NovaLab (ReqFun AS LONG,_
                 PosIndex AS LONG,_
                 SkuIndex AS LONG,_
                 TOrd AS OrdSummRec,_
                 TDetail AS DetailRec,_
                 DetCurStep AS LONG) EXPORT
        DIM IHandle  AS LONG
        DIM WORK     AS STRING
        DIM myUserArea AS STRING
        DIM myUserArea1 AS STRING
        DIM userLen AS LONG

' *************************************************************************************
    DIM MSG AS STRING
    DIM FILENUM AS LONG
    DIM SKU AS STRING
    DIM DESC AS STRING
    DIM QTY AS STRING
    DIM EMPID AS STRING
    DIM ORDER AS STRING
    DIM INVOICE AS STRING
    DIM LOADDATE AS STRING
    DIM ORIGIN AS STRING
    DIM X AS LONG
    DIM NEWWORK AS STRING
    DIM WORK_YEAR AS STRING
    DIM WORK_MO AS STRING
    DIM WORK_DAY AS STRING
    DIM UPC AS STRING
    DIM key AS STRING
    DIM recFound AS LONG
    DIM PrintUPC AS LONG
    DIM True AS LONG
    DIM False AS LONG
    DIM PrinterIP AS STRING
    DIM PrinterPort AS STRING
    DIM iERR AS LONG
    DIM hSocket AS LONG
        DIM ADJQTY AS LONG

    True = -1
    False = 0

' 9/24/2015
        'These files and MILNOVASEX are used to upload each pick back to host
        'They are not finished or in production as of 9/25/2015
        'Maybe never, but are not complete or tested
        '$INCLUDE "RECORDERROR.BAS"
        '$INCLUDE "WRITEUPLOADRECORD.BAS"
        '$INCLUDE "PROCESSUPLOAD.BAS"
        '$INCLUDE "CREATEUPLOADRECORD.BAS"

   ' CREATEUPLOADRECORD(ReqFun, PosIndex, SkuIndex, TOrd, TDetail, DetCurStep)

   ' PROCESSUPLOAD

    'Get the UPC-A number if it has one
    PrintUPC = False
    recFound = False
    key = RIGHT$( SPACE$(35) + TRIM$(TDetail.sku) ,35)

    AKAAcc 1, key, 0, RecFound,0,AKA
    IF recFound THEN
      UPC = TRIM$(AKA.AKASKU)
      IF LEN(UPC) => 11 THEN
        UPC = LEFT$(UPC,11)
        PrintUPC = True
      END IF
    END IF

    IF (LEFT$(LTRIM$(RTRIM$(TDetail.SKU)), 2) = "48") AND (ReqFun <> 2) THEN
        EXIT SUB                   'label print is not for "48" parts unless reprint
    END IF

    IF (LEFT$(LTRIM$(RTRIM$(TDetail.SKU)), 2) = "49") AND (ReqFun <> 2) THEN
        EXIT SUB                   'label print is not for "49" parts unless reprint
    END IF

    FILENUM = FREEFILE
    OPEN "PRINTERNAME.TXT" FOR INPUT AS #FILENUM
    INPUT #FILENUM, PrinterPort, PrinterIP
    CLOSE #FILENUM

    SKU = LTRIM$(RTRIM$(TDetail.SKU))
    DESC =LTRIM$(RTRIM$(TDetail.DES))
'***********************************************
'Change to make adjusted qty's show up on label
' Jan 18, 2017
    IF TDetail.DbOvrFlag(1) = TRUE THEN
        QTY = FORMAT$(TDetail.DbOvrQty(1))
    ELSE
        QTY = FORMAT$(TDetail.Qty)
    END IF

    'QTY = STR$(TDetail.Qty)
    'QTY = LTRIM$(RTRIM$(QTY))
'*************************************************
    EMPID = TOrd.PickEmp
    EMPID = LTRIM$(RTRIM$(EMPID))
    ORDER = TOrd.Order
    ORDER = LTRIM$(RTRIM$(ORDER))
    'INVOICE = TOrd.Invoice
    'INVOICE = LTRIM$(RTRIM$(INVOICE))
    INVOICE = "          "
    WORK_YEAR = MID$(TOrd.LOADDate,1,4)
    WORK_MO = MID$(TOrd.LOADDate,5,2)
    WORK_DAY = MID$(TOrd.LOADDate,7,2)
    LOADDATE = WORK_MO + "-" + WORK_DAY + "-" + WORK_YEAR
    ORIGIN = MID$(TDetail.UserArea,20,20)
    ORIGIN = LTRIM$(RTRIM$(ORIGIN))

    MSG = MSG + "^XA"
    IF PrintUPC THEN
      MSG = MSG + "^FO465,010^BY3^BUN,150^FD" + UPC + "^FS"
    ELSE
      MSG = MSG + "^FO25,010^B3N,N,150,N,N^FD" + SKU + "^FS"
    END IF
    MSG = MSG + "^FO025,175^A0N,100,105^FD" + SKU + "^FS"
    MSG = MSG + "^FO25,260^ABN,25,14^FD" + DESC + "^FS"
    MSG = MSG + "^FO25,290^ABN,25,14^FDQTY: " + QTY + "        *" + EMPID + "*^FS"
    MSG = MSG + "^FO25,325^ABN,25,14^FDM" + ORDER + "  " + INVOICE + " * " + LOADDATE + "^FS"
    MSG = MSG + "^FO25,360^ABN,25,14^FDMADE IN " + ORIGIN + "^FS"
    MSG = MSG + "^XZ"

    hSocket = FREEFILE
    ERRCLEAR
    TCP OPEN PORT VAL(PrinterPort)  AT PrinterIP AS hSocket TIMEOUT 60000
    IF ERR THEN
       iERR=ERR
       TCP CLOSE hsocket
       MSGBOX "Open the printer TCP socket error code: "+STR$(iERR)
       IF iERR=57 THEN MSGBOX "Could not find printer"
       EXIT SUB
    END IF
    TCP SEND hSocket, MSG
    IF ERR THEN
        iERR=ERR
        TCP CLOSE hSocket
        MSGBOX "TCP send error code: "+STR$(iERR)
        EXIT SUB
    END IF
    ERRCLEAR
    TCP CLOSE hSocket
    IF ERR THEN
        iERR=ERR
        MSGBOX "Close the printer TCP socket Error code: "+STR$(iERR)
        EXIT SUB
    END IF
    SLEEP 500
END SUB
