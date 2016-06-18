Import-Module ".\packages\MagicChunks.0.5.9.23\lib\net45\MagicChunks.psm1";

Task Build {
        Format-MagicChunks -path Web.config -target Web.transformed.1.config -transformations @{
            "configuration/system.web/authentication/@mode" = "Forms";
            "configuration/system.web/httpRuntime" = "125";
            "configuration/appSettings/add[@key='LoadBundledScripts']/@value" = "true";
            "configuration/appSettings/add[@key='SomethingNew']/@value" = "NewValue";
            "configuration/newKey" = "12345"
        }

        Format-MagicChunks -path Web.config -target Web.transformed.2.config -transformations @{
            "configuration/system.web/authentication/@mode" = "Forms";
            "configuration/system.web/httpRuntime" = "125";
            "configuration/appSettings/add[@key='LoadBundledScripts']/@value" = "true";
            "configuration/appSettings/add[@key='SomethingNew']/@value" = "NewValue";
            "configuration/newKey" = "12345"
        } -type Xml

        Format-MagicChunks -path config.json -target config.transformed.1.json -transformations @{
            "Data/DefaultConnection/ConnectionString" = "mongodb://10.1.25.144/";
            "Data/DefaultConnection/Production" = "true";
            "Smtp/Method" = "Network";
            "NewKey" = "12345"
        }

        Format-MagicChunks -path config.json -target config.transformed.2.json -transformations @{
            "Data/DefaultConnection/ConnectionString" = "mongodb://10.1.25.144/";
            "Data/DefaultConnection/Production" = "true";
            "Smtp/Method" = "Network";
            "NewKey" = "12345"
        } -type Json

        Format-MagicChunks -path _config.yml -target _config.transformed.1.yml -transformations @{
            "baseUrl" = "http://production.com/";
            "frontend_version" = "3.0.5";
            "new_key" = "23";
            "another_key/val/t" = "qwerty"
        }

        Format-MagicChunks -path _config.yml -target _config.transformed.2.yml -transformations @{
            "baseUrl" = "http://production.com/";
            "frontend_version" = "3.0.5";
            "new_key" = "23";
            "another_key/val/t" = "qwerty"
        } -type Yaml
}

Task Default -depends Build;