#addin "MagicChunks"

var target = Argument("target", "Default");

Task("Default")
    .Does(() => {

        TransformConfig(@"Web.config", "Web.transformed.1.config", new TransformationCollection {
            { "configuration/system.web/authentication/@mode", "Forms" },
            { "configuration/system.web/httpRuntime", "125" },
            { "configuration/appSettings/add[@key='LoadBundledScripts']/@value", "true" },
            { "configuration/appSettings/add[@key='SomethingNew']/@value", "NewValue" },
            { "configuration/newKey", "12345" }
        });

        TransformConfig("Xml", @"Web.config", "Web.transformed.2.config", new TransformationCollection {
            { "configuration/system.web/authentication/@mode", "Forms" },
            { "configuration/system.web/httpRuntime", "125" },
            { "configuration/appSettings/add[@key='LoadBundledScripts']/@value", "true" },
            { "configuration/appSettings/add[@key='SomethingNew']/@value", "NewValue" },
            { "configuration/newKey", "12345" }
        });

        TransformConfig(@"config.json", "config.transformed.1.json", new TransformationCollection {
            { "Data/DefaultConnection/ConnectionString", "mongodb://10.1.25.144/" },
            { "Data/DefaultConnection/Production", "true" },
            { "Smtp/Method", "Network" },
            { "NewKey", "12345" }
        });

        TransformConfig("Json", @"config.json", "config.transformed.2.json", new TransformationCollection {
            { "Data/DefaultConnection/ConnectionString", "mongodb://10.1.25.144/" },
            { "Data/DefaultConnection/Production", "true" },
            { "Smtp/Method", "Network" },
            { "NewKey", "12345" }
        });

        TransformConfig(@"_config.yml", "_config.transformed.1.yml", new TransformationCollection {
            { "baseUrl", "http://production.com/" },
            { "frontend_version", "3.0.5" },
            { "new_key", "23" },
            { "another_key/val/t", "qwerty" }
        });

        TransformConfig("Yaml", @"_config.yml", "_config.transformed.2.yml", new TransformationCollection {
            { "baseUrl", "http://production.com/" },
            { "frontend_version", "3.0.5" },
            { "new_key", "23" },
            { "another_key/val/t", "qwerty" }
        });

    });

RunTarget(target);