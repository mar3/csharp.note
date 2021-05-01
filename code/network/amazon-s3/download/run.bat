@SETLOCAL

@SET AWS_ACCESS_KEY_ID=XXXXXXXXXXXXXXXXXXXX
@SET AWS_SECRET_ACCESS_KEY=xXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxXxX

@CALL bin\Release\netcoreapp3.1\download.exe
@CALL bin\Release\netcoreapp3.1\download.exe "%SRC%" "%DEST%"
