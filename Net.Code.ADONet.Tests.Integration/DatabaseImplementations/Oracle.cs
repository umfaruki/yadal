using System;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace Net.Code.ADONet.Tests.Integration
{
    public class Oracle : BaseDb
    {
        public override string CreatePersonTable => "CREATE TABLE PERSON (" +
                                                    "    ID NUMBER(8,0) NOT NULL" +
                                                    ",   OPTIONAL_NUMBER NUMBER(8,0) NULL" +
                                                    ",   REQUIRED_NUMBER NUMBER(8,0) NOT NULL" +
                                                    ",   NAME VARCHAR2(100) NOT NULL" +
                                                    ",   EMAIL VARCHAR2(100) NOT NULL" +
                                                    ")";
        public override string CreateAddressTable => "CREATE TABLE ADDRESS (" +
                                                    "    ID NUMBER(8,0) NOT NULL" +
                                                    ",   STREET VARCHAR2(100) NOT NULL" +
                                                    ",   ZIP_CODE VARCHAR2(20) NOT NULL" +
                                                    ",   CITY VARCHAR2(100) NOT NULL" +
                                                    ",   COUNTRY VARCHAR2(100) NOT NULL" +
                                                    ")";

        public override string InsertPerson => $"INSERT INTO PERSON (ID,OPTIONAL_NUMBER,REQUIRED_NUMBER,NAME,EMAIL) VALUES (:Id,:OptionalNumber,:RequiredNumber,:Name,:Email)";

        public override string InsertAddress => $"INSERT INTO ADDRESS (ID,STREET,ZIP_CODE,CITY,COUNTRY) VALUES (:Id,:Street,:ZipCode,:City,:Country)";

        public override void DropAndRecreate()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[Name].ConnectionString;
            var connectionStringBuilder = new OracleConnectionStringBuilder
            {
                ConnectionString = connectionString
            };
            var databaseName = connectionStringBuilder.UserID;
            var ddl = string.Format("DECLARE\r\n" +
                                    "\r\n" +
                                    "    c INTEGER := 0;\r\n" +
                                    "\r\n" +
                                    "BEGIN\r\n" +
                                    "    SELECT count(*) INTO c FROM sys.dba_users WHERE USERNAME = \'{0}\';\r\n" +
                                    "    IF c = 1 THEN\r\n" +
                                    "            execute immediate (\'drop user {0} cascade\');\r\n" +
                                    "            execute immediate (\'drop tablespace {0}_TS\');\r\n" +
                                    "            execute immediate (\'drop tablespace {0}_TS_TMP\');\r\n" +
                                    "    END IF;\r\n" +
                                    "    \r\n" +
                                    "    execute immediate (\'create tablespace {0}_TS datafile \'\'{0}.dat\'\' size 10M reuse autoextend on\');\r\n" +
                                    "    execute immediate (\'create temporary tablespace {0}_TS_TMP tempfile \'\'{0}_TMP.dat\'\' size 10M reuse autoextend on\');\r\n" +
                                    "    execute immediate (\'create user {0} identified by pass default tablespace {0}_TS temporary tablespace {0}_TS_TMP\');\r\n" +
                                    "    execute immediate (\'grant create session to {0}\');\r\n" +
                                    "    execute immediate (\'grant create table to {0}\');\r\n" +
                                    "    execute immediate (\'GRANT UNLIMITED TABLESPACE TO {0}\');\r\n" +
                                    "\r\n" +
                                    "END;", databaseName);

            using (var db = Db.FromConfig(MasterName))
            {
                db.Execute(ddl);
            }

        }

        protected override Type ProviderType => typeof (OracleClientFactory);
        public override string EscapeChar => ":";
        public override Person Project(dynamic d)
        {
            return new Person
            {
                Id = d.ID,
                Email = d.EMAIL,
                Name = d.NAME,
                OptionalNumber = d.OPTIONAL_NUMBER,
                RequiredNumber = d.REQUIRED_NUMBER
            };
        }
    }
}