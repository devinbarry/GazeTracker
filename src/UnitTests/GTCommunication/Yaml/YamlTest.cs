// <copyright>
// ******************************************************
// UnitTests for ITU GazeTracker
// Copyright (C) 2011 Dot Au Pty Limited
// ------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation; either version 3 of the License, 
// or (at your option) any later version.
// This program is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
// General Public License for more details.
// You should have received a copy of the GNU General Public License 
// along with this program; if not, see http://www.gnu.org/licenses/.
// ------------------------------------------------------------------------
// Modified by Alastair Jeremy
// **************************************************************
// </copyright>
// <author>Jonathan Slenders and Christophe Lambrechts</author>
// <email>gaze@dotaussie.com.au</email>
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using NUnit.Framework;
using UnitTests;
using log4net;
using System.Xml;
using System.IO;

namespace GTCommunication
{
    [TestFixture]
    public class YamlTest
    {
        #region Logging Setup
        /// <summary>
        /// Logger for log4net logging
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );
        /// <summary>
        /// Indicator that can be used for high speed DEBUG level logging. Note that using this flag will prevent automated reload
        /// of log4net configuration for that log statement during program operation
        /// </summary>
        private static readonly bool isDebugEnabled = log.IsDebugEnabled;
        #endregion

        //[Test]
        //public void testYamlReadingFromFile()
        //{
        //    try
        //    {
        //        Node node = Node.FromFile( "TestResources/Yaml/test.yaml" );
        //    }
        //    catch ( ParseException e )
        //    {
        //        Console.WriteLine( "[ERROR]" );
        //        Console.WriteLine( e );
        //        Console.WriteLine( "[/ERROR]" );
        //        Assert.Fail( e.ToString() );
        //    }
        //}

        //[Test]
        //public void testYamlInterpretationForDataSetPointList()
        //{
        //    Node node1 = Node.FromFile( "TestResources/Yaml/landmark-1.yml" );
        //    Node node2 = Node.FromFile( "TestResources/Yaml/regions-1.yml" );
        //    Node node3 = Node.FromFile( "TestResources/Yaml/regions2-1.yml" );

        //    Console.WriteLine( node1.ToString() );

        //    Console.WriteLine( "--" );
        //    Console.WriteLine( node1.Info() );

        //    // Now verify the node contents and how to access the sub-parts
        //    Assert.AreEqual( node1.Type, NodeType.Mapping );
        //    Mapping m = ( Mapping ) node1;
        //    Assert.AreEqual( 1, m.Count, "Wrong number of mappings" );

        //    Console.WriteLine( "--" );
        //    Console.WriteLine( m.Info() );
        //    Console.WriteLine( "--" );

        //}

        //[Test]
        //public void testBasicYamlOperation()
        //{
        //    List<string> parse = new List<string>();

        //    // In this functions is some test data added to a Arraylist of strings
        //    Varia( parse );
        //    Float( parse );
        //    Timestamp( parse );
        //    Binary( parse );
        //    Null( parse );
        //    Boolean( parse );
        //    Integer( parse );
        //    String( parse );

        //    // Parse all items in ArrayList, catches ParseException, ensure no errors in this list
        //    foreach ( string s in parse )
        //    {
        //        // Console.WriteLine( "Parsed = >>" + s + "<<" );
        //        try
        //        {
        //            Node testNode = Node.Parse( s );
        //            // Console.WriteLine( testNode.ToString() );
        //            // Console.WriteLine( "Yaml result = " + testNode.Write() );
        //        }
        //        catch ( ParseException e )
        //        {
        //            Console.WriteLine( "[ERROR]" );
        //            Console.WriteLine( e );
        //            Console.WriteLine( "[/ERROR]" );
        //            Assert.Fail( e.ToString() );
        //        }
        //        // Console.WriteLine( "--------------------------------------------------" );
        //    }

        //    // Ensure we always get errors when expected
        //    parse = new List<string>();
        //    Errors( parse );

        //    foreach ( string s in parse )
        //    {
        //        // Console.WriteLine( "Parsed = >>" + s + "<<" );
        //        try
        //        {
        //            Node testNode = Node.Parse( s );
        //            Assert.Fail( "This should actually cause an error: " + s );
        //        }
        //        catch ( ParseException e )
        //        {
        //            // this is expected
        //        }
        //        // Console.WriteLine( "--------------------------------------------------" );
        //    }
        //}

        #region YAML Sample Strings

        private static void Varia( List<string> parse )
        {
            parse.Add( "- \"test\"" );
        }

        private static void Float( List<string> parse )
        {
            parse.Add( "!!float -.inf" );
            parse.Add( "+.inf" );
            parse.Add( "+.Inf" );
            parse.Add( "-.INF" ); //this is a problem, without !!float it is interpreted as sequence
            parse.Add( "!!float .NaN" );
            parse.Add( ".nan" );
            parse.Add( "!!float .NAN" );
            parse.Add( "10.89" );
            parse.Add( "19999380.898300090000000000" ); // The result is 19999380.8983001, so there is a rounding. Is this bad??
            parse.Add( "19999380.89830009" ); // Proof that 000 after the 9 doesn't matter
            parse.Add( "80.8983000900000000000" );
            parse.Add( "-80.8983000900000000001" ); // No rounding
            parse.Add( "20000000000000.0" ); // Will fit in an integer, force to put in float
            parse.Add( "9223372036854775810" ); // Don't fit in an integer, but will fit in float
            parse.Add( "!!float 32300.1E-39" );
            parse.Add( "!!float 32332.1e+39" );
            parse.Add( "323:20:32.139" ); //  ||
            parse.Add( "6.8523015e+5" );  //  \/
            parse.Add( "685.230_15e+03" ); // The same number in different notations: http://yaml.org/type/float.html
            parse.Add( "685_230.15" ); //     /\
            parse.Add( "190:20:30.15" ); //   ||
        }

        private static void Timestamp( List<string> parse )
        {
            parse.Add( "2001-12-15T02:59:43" );
            parse.Add( "2001-12-14t21:59:43" );
            parse.Add( "2001-12-14 21:59:43" );
            parse.Add( "!!timestamp 2001-12-15		21:59:43" );
            parse.Add( "2002-12-14" );
            parse.Add( "2001-12-15T2:59:43" );
            parse.Add( "2001-12-15T02:59:43.5Z" );
            parse.Add( "2001-12-15T02:59:43.05Z" );
            parse.Add( "2001-12-14t21:59:43.10-05:00" );
            parse.Add( "!!timestamp 2001-12-14 21:59:43.10 -5" );
            parse.Add( "!!timestamp 2001-12-14 21:59:43.10 -5:10" );
            parse.Add( "!!timestamp 2001-12-14 21:59:43.3333333333333+5:10" );
            parse.Add( "2001-12-15 2:59:43.10" );
        }

        private static void Errors( List<string> parse )
        {
            // Fault on next item
            parse.Add( "!!timestamp 2001-12-15		24:59:43" );

            //This gives an error, this is right, if tags used then content after it must be correct
            parse.Add( "!!null test" );

            //To large, force in int (gives error)
            parse.Add( "!!int 9223372036854775808" );
        }

        private static void Binary( List<string> parse )
        {
            parse.Add( "!!binary \"R0lGODlhDAAMAIQAAP//9/X17unp5WZmZgAAAOfn515eXvPz7Y6OjuDg4J+fn5" +
                "OTk6enp56enmlpaWNjY6Ojo4SEhP/++f/++f/++f/++f/++f/++f/++f/++f/++" +
                "f/++f/++f/++f/++f/++SH+Dk1hZGUgd2l0aCBHSU1QACwAAAAADAAMAAAFLC" +
                "AgjoEwnuNAFOhpEMTRiggcz4BNJHrv/zCFcLiwMWYNG84BwwEeECcgggoBADs=\"" ); // A little gif arrow
            parse.Add( "!!binary \"iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAMAAAAoLQ9TAAAABGdBTUEAAK/INwWK6QAAABl0RVh0U29mdHdhcmUAQWRvYmUgSW1hZ2VSZWFkeXHJZTwAAAGbUExUReqNUcmie8ajf6trO86gdeicU8ijfOiHU8ukeeiNU8eee+uJT+iJU+eKVOaMUuaLVs6cc6RlN8yjeNeaacuieeeIU+qLUeSSWuaPVueOVOqHUdWda8iHSOCQXOqcUfPz88h7SdN9R+KYXM+UW5ZpPduGSt+YYOeUVsGCReGPW9KESdGdbsmgediRULiARb2bcdyXV8iAS8h6SMyleNGZbtCDT9Wfa8ukeuiUU+KOTsx8QtibZuuQT9uTZOiRU8N6Q+eKVpNfMuuOT+eTVOiWU8qjes6jdalvPY9rRMaff+eIVOeXVuKITuaJVuCUXM+dcsx3QuOLTMedb8meeeOCTM6edduWZOqJUdSXbMN5QsyfdtKeb+icVLWGVMqfeOCQXqltPaVnNaJtOuCWXsp4RuSUWtedach7SMmgfOSQWOKWTtaWXtN5R6JpOpVqPZRpP76BS9iVZueQVuSSWKtoOr6DSqtqOpdpPNucYuyZTrt0P6h0P9uYYsegfJBkN6tsOs+bctCgc+OPVd+PVN+ZU8ege+eMVpGRj////+L+QKQAAAEtSURBVHjaYkiTjmbhaGViam2NEzAplJUHCCAG6cZQIUY2NiZGjqiGCFvZdoAAYmBhEeJgZGPq0A/SFg6LF28HCCAGERFXxozajo6O2Ozk1OK2doAAYuDiam017+jw6OgoaLbglGwHCCAGLs9W446OfJXqjg57SwbOdoAAYgjWcevoyCsv1dXo6Ij0YWgHCCAGAX8z5Y46vZaYrA7HMlWGdoAAYhC2rjDoSJJhZZXrSKwKZGgHCCCGGjUl7w53GVa5yo56KxvudoAAYvATV3fpgADBACfudoAAYtDMdLbTYs5NYGYWLFHkFmsHCCCGtiJOhiZTw3QFo5ScECmxdoAAYuDl4eER5eOTkJBklwpn520HCCCGNl4vdlFffv62tjYHIG4HCCAG+XZUABBgAB0+SyCFLN5aAAAAAElFTkSuQmCC\"" ); // A house
            parse.Add( "!!binary | \n" +
                "AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8gISIjJCUmJygpKissLS4vMDEyMzQ1Njc4\n" +
                "OTo7PD0+P0BBQkNERUZHSElKS0xNTk9QUVJTVFVWV1hZWltcXV5fYGFiY2RlZmdoaWprbG1ub3Bx\n" +
                "cnN0dXZ3eHl6e3x9fn+AgYKDhIWGh4iJiouMjY6PkJGSk5SVlpeYmZqbnJ2en6ChoqOkpaanqKmq\n" +
                "q6ytrq+wsbKztLW2t7i5uru8vb6/wMHCw8TFxsfIycrLzM3Oz9DR0tPU1dbX2Nna29zd3t/g4eLj\n" +
                "5OXm5+jp6uvs7e7v8PHy8/T19vf4+fr7/P3+/w==" );
        }

        private static void Null( List<string> parse )
        {
            parse.Add( "" );
            parse.Add( "~" );
            parse.Add( "null" );
            parse.Add( "Null" );
            parse.Add( "NULL" );
            parse.Add( "!!null" );
            parse.Add( "!!null ~" );
        }

        private static void Boolean( List<string> parse )
        {
            parse.Add( "y" );
            parse.Add( "Y" );
            parse.Add( "yes" );
            parse.Add( "Yes" );
            parse.Add( "YES" );
            parse.Add( "n" );
            parse.Add( "N" );
            parse.Add( "no" );
            parse.Add( "No" );
            parse.Add( "NO" );
            parse.Add( "true" );
            parse.Add( "True" );
            parse.Add( "false" );
            parse.Add( "False" );
            parse.Add( "FALSE" );
            parse.Add( "on" );
            parse.Add( "On" );
            parse.Add( "ON" );
            parse.Add( "off" );
            parse.Add( "Off" );
            parse.Add( "OFF" );
        }

        private static void Integer( List<string> parse )
        {
            parse.Add( "!!int -0xA_7f10c" );
            parse.Add( "+0xA_7f10c" );
            parse.Add( "123456789" );
            parse.Add( "200000000:11:50:59" );
            parse.Add( "-200000000:11:50:59" ); //Problem with sequence
            parse.Add( "2000000000" );
            parse.Add( "02472256" ); // Octal
            parse.Add( "0b1010_0111_0100_1010_1110" ); // Binary
            parse.Add( "9223372036854775807" ); //Max value
            parse.Add( "-9223372036854775808" ); //Min value, still problem with sequence
            parse.Add( "9223372036854775808" ); //To large, try to fit float
            parse.Add( "2147483647" ); //Max value int32
        }

        private static void String( List<string> parse )
        {
            parse.Add( "Test \"te" );
            parse.Add( "!!str | \n abc \n def" );
            parse.Add( "\"test that it is possible to use double quotes \\\" ... \"" );
            parse.Add( "'test a '' quote in single quotes'" );
            parse.Add( "test: hello" );
            parse.Add( "':/%.&(*'" );
            parse.Add( "\"newlines test, \\nfirst\\nsecond\"" );
            parse.Add( "- test\n" +
                "  hallo" );
            parse.Add( "key:\n \"String over more then\n one line\"" );
            parse.Add( "? test: '#:/%.&(*'" );
        }

        #endregion


    }
}