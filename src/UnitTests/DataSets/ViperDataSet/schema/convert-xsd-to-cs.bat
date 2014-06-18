xsd viper-sample.xml
xsd viper-sample.xsd viper-sample_app1.xsd /classes /namespace:UnitTests.Viper.Sample
del viper-sample.cs
rename viper-sample_viper-sample_app1.cs viper-sample.cs

xsd viper.xsd viperdata.xsd /classes /namespace:UnitTests.Viper.Schema
del viper.cs
rename viper_viperdata.cs viper.cs
pause
