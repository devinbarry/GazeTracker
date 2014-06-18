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
// **************************************************************
// </copyright>
// <author>Alastair Jeremy</author>
// <email>gaze@dotaussie.com.au</email>
using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using System.IO;
using NUnit.Framework;
using System.Drawing;
using UnitTests.DataSetMediaApi;
using UnitTests.DataSetMediaImpl;
using QiHe.Yaml.Grammar;
using GTCommunication.YamlUtility;
using System.Xml.Serialization;
using System.Xml;

using UnitTests.Viper.Sample;

namespace UnitTests.DataSets
{
    public class ViperDataSet : PrivateDataSet
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

        private static List<IMediaSequence> _mediaSequences = null;

        private string _viperDataFilename = null;

        private static long missingFiles = 0;
        private static long yamlErrors = 0;
        private static long contourFiles = 0;
        private static long regionAllZero = 0;
        private static long notFoundRegionCorners = 0;
        private static long unrecognisedFeatureName = 0;
        private static long noContours = 0;

        private static Object obj = new Object();

        protected ViperDataSet( string name, string viperDataFilename )
            : base( name )
        {
            _viperDataFilename = viperDataFilename;
        }

        public override IEnumerable<IMediaSequence> getMediaSequences()
        {
            lock ( obj )
            {
                if ( _mediaSequences == null )
                {
                    _mediaSequences = new List<IMediaSequence>();
                    log.Debug( "Add the media sequence specified by the Viper config file (& associated .info files)" );
                    _mediaSequences.AddRange( viperLabelledFeatures() );
                }
            }
            return _mediaSequences;
        }

        private UnitTests.Viper.Sample.viper deserialiseObject( string filename )
        {
            log.Debug( "deserializeObject: Reading with XmlReader" );
            // Create an instance of the XmlSerializer specifying type and namespace.
            XmlSerializer serializer = new XmlSerializer( typeof( UnitTests.Viper.Sample.viper ) );
            // A FileStream is needed to read the XML document.
            FileStream fs = new FileStream( filename, FileMode.Open );
            XmlReader reader = new XmlTextReader( fs );
            // Declare an object variable of the type to be deserialized & use the Deserialize method to restore the object's state.
            UnitTests.Viper.Sample.viper vt = ( UnitTests.Viper.Sample.viper ) serializer.Deserialize( reader );
            reader.Close();
            fs.Close();
            return vt;
        }

        private double? extractAttributeDouble( attribute[] attributes, string name )
        {
            attribute a = extractAttribute( attributes, name );
            if ( ( a == null ) || ( a.fvalue == null ) )
            {
                return null;
            }
            try
            {
                return Convert.ToDouble( a.fvalue.value );
            }
            catch ( Exception )
            {
                return null;
            }
        }

        private long? extractAttributeLong( attribute[] attributes, string name )
        {
            attribute a = extractAttribute( attributes, name );
            if ( ( a == null ) || ( a.dvalue == null ) )
            {
                return null;
            }
            try
            {
                return Convert.ToInt64( a.dvalue.value );
            }
            catch ( Exception )
            {
                return null;
            }
        }

        private attribute extractAttribute( attribute[] attributes, string name )
        {
            foreach ( attribute a in attributes )
            {
                if ( a.name.CompareTo( name ) == 0 )
                {
                    return a;
                }
            }
            return null;
        }

        private viperConfigDescriptor extractViperConfigDescriptorByName( viperConfigDescriptor[] descriptors, string name )
        {
            foreach ( viperConfigDescriptor d in descriptors )
            {
                if ( d.name.CompareTo( name ) == 0 )
                {
                    return d;
                }
            }
            return null;
        }

        private viperConfigDescriptor extractViperConfigDescriptorByType( viperConfigDescriptor[] descriptors, string type )
        {
            foreach ( viperConfigDescriptor d in descriptors )
            {
                if ( d.type.CompareTo( type ) == 0 )
                {
                    return d;
                }
            }
            return null;
        }

        private viperDataSourcefileFile extractViperDataSourceFileFile( viperDataSourcefileFile[] files, string name )
        {
            foreach ( viperDataSourcefileFile f in files )
            {
                if ( f.name.CompareTo( name ) == 0 )
                {
                    return f;
                }
            }
            return null;
        }

        private List<KeyValuePair<string, string>> extractViperSampleTypes( attribute[] attributes )
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            foreach ( attribute a in attributes )
            {
                KeyValuePair<string, string> storedType = new KeyValuePair<string, string>( a.name, a.type );
                list.Add( storedType );
            }
            return list;
        }

        private IEnumerable<IMediaSequence> viperLabelledFeatures()
        {
            List<IMediaSequence> mediaSequence = new List<IMediaSequence>();

            UnitTests.Viper.Sample.viper vt = deserialiseObject( _viperDataFilename );

            List<KeyValuePair<string, string>> possibleAttributes = new List<KeyValuePair<string, string>>();
            foreach ( object o in vt.Items )
            {
                if ( o is viperConfig )
                {
                    // Extract a list of all the possible attributes for all the possible objects.
                    // This does make an assumption that they are all uniquely named, and all apply to all frames. 
                    // This assumption is not valid generally, but is OK for our situation
                    viperConfig vc = ( viperConfig ) o;
                    foreach ( viperConfigDescriptor d in vc.descriptor )
                    {
                        if ( d.type.CompareTo( "OBJECT" ) == 0 )
                        {
                            possibleAttributes.AddRange( extractViperSampleTypes( d.attribute ) );
                        }
                    }
                }
            }
            // Now we have compiled a list of all possible attributes for all possible objects

            // Go through the viperData itself now & extract the individual items
            foreach ( object o in vt.Items )
            {
                if ( o is viperData )
                {
                    //
                    log.Debug( "Found the viperData" );
                    viperData vd = ( UnitTests.Viper.Sample.viperData ) o;

                    foreach ( viperDataSourcefile vsf in vd.sourcefile )
                    {
                        String filenameFrameImages = null;
                        filenameFrameImages = vsf.filename;
                        viperDataSourcefileFile vsff = extractViperDataSourceFileFile( vsf.file, "Information" );
                        long? numFrames = extractAttributeLong( vsff.attribute, "NUMFRAMES" );

                        // Loop through all the objects
                        foreach ( viperDataSourcefileObject sfo in vsf.@object )
                        {
                            // Go through each of the named attributes
                            foreach ( KeyValuePair<string, string> kvp in possibleAttributes )
                            {
                                attribute a = extractAttribute( sfo.attribute, kvp.Key );
                                if ( a != null )
                                {
                                    // Get all the points & add them to the relevant MediaSequence entries for that named attribute
                                    bool extracted = false;

                                    // have to do it based on the type obtained in kvp.Value
                                    if ( kvp.Value.CompareTo( "http://lamp.cfar.umd.edu/viperdata#point" ) == 0 )
                                    {
                                        if ( a.point == null )
                                        {
                                            log.Debug( "Unable to properly extract the attribute" );
                                        }
                                        else
                                        {
                                            // The problem here is that we only seem to be seeing the first point



                                            // ... TODO ...

                                            // Pull out a point version
                                            // ...
                                            // Mark as dealt with
                                            extracted = true;
                                        }
                                    }

                                    if ( kvp.Value.CompareTo( "http://lamp.cfar.umd.edu/viperdata#bbox" ) == 0 )
                                    {
                                        if ( a.point == null )
                                        {
                                            log.Debug( "Unable to properly extract the attribute" );
                                        }
                                        else
                                        {
                                            // ... TODO ...

                                            // Pull out a bounding box version
                                            // ...
                                            // Mark as dealt with
                                            extracted = true;
                                        }
                                    }

                                    if ( kvp.Value.CompareTo( "http://lamp.cfar.umd.edu/viperdata#circle" ) == 0 )
                                    {
                                        if ( a.point == null )
                                        {
                                            log.Debug( "Unable to properly extract the attribute" );
                                        }
                                        else
                                        {
                                            // ... TODO ...

                                            // Pull out a circle version
                                            // ...
                                            // Mark as dealt with
                                            extracted = true;
                                        }
                                    }

                                    // ... TODO ...
                                }
                            }
                        }

                        // Now go to the next source file level (source file is actually a list of image files)
                    }
                }
            }






            UnitTests.Viper.Schema.viper vtt = UnitTests.Viper.Schema.viper.LoadFromFile( _viperDataFilename );

            for ( int subject = 1; subject <= 100; subject++ )
            {
                string region1BaseDirectory = directory() + "/regions/R" + subject.ToString( "000" );
                string region2BaseDirectory = directory() + "/regions2/R" + subject.ToString( "000" );
                string landmarkBaseDirectory = directory() + "/landmarks/L" + subject.ToString( "000" );
                string landmark2BaseDirectory = directory() + "/landmarks2/L" + subject.ToString( "000" );
                string contourBaseDirectory = directory() + "/contours/C" + subject.ToString( "000" );

                string imageBaseDirectory = directory() + "/images/0" + subject.ToString( "000" );

                DirectoryInfo region1Dir = new DirectoryInfo( region1BaseDirectory );
                DirectoryInfo region2Dir = new DirectoryInfo( region2BaseDirectory );
                DirectoryInfo landmarkDir = new DirectoryInfo( landmarkBaseDirectory );
                DirectoryInfo landmark2Dir = new DirectoryInfo( landmark2BaseDirectory );
                DirectoryInfo contourDir = new DirectoryInfo( contourBaseDirectory );

                FileInfo[] region1Files = region1Dir.GetFiles( "*.yml" );
                foreach ( FileInfo region1FileInfo in region1Files )
                {
                    string imageFilename = imageBaseDirectory + "/" + region1FileInfo.Name.Replace( ".yml", ".JPG" );
                    bool imageFileFound = File.Exists( imageFilename );

                    string region2Filename = region2BaseDirectory + "/" + region1FileInfo.Name;
                    bool region2FileFound = File.Exists( region2Filename );

                    string landmarkFilename = landmarkBaseDirectory + "/" + region1FileInfo.Name;
                    bool landmarkFileFound = File.Exists( landmarkFilename );

                    string landmark2Filename = landmark2BaseDirectory + "/" + region1FileInfo.Name;
                    bool landmark2FileFound = File.Exists( landmark2Filename );

                    string contourFilename = contourBaseDirectory + "/" + region1FileInfo.Name;
                    bool contourFileFound = File.Exists( contourFilename );

                    if ( contourFileFound )
                    {
                        contourFiles++;
                    }

                    if ( !imageFileFound )
                    {
                        missingFiles++;
                        break;
                    }

                    if ( !region2FileFound )
                    {
                        // Have Region 1 file, have image file, but missing Region 2 file
                        missingFiles++;
                        break;
                    }

                    if ( !landmarkFileFound )
                    {
                        // Have Region 1 file, Region 2 file, have image file, but missing Landmark file
                        missingFiles++;
                        break;
                    }

                    if ( !landmark2FileFound )
                    {
                        // Have Region 1 file, Region 2 file, have image file, but missing Landmark file
                        missingFiles++;
                        break;
                    }

                    // We have the corresponding image file, now we actually need to load and process all 3 YAML files (which also exist)
                    bool successRegion1 = false;
                    bool successRegion2 = false;
                    bool successLandmark = false;
                    bool successLandmark2 = false;
                    bool successContour = false;
                    YamlParser parser = new YamlParser();

                    //TextInput inputRegion1 = new TextInput( yamlPreprocessor( File.ReadAllText( region1FileInfo.FullName ) ) );
                    //YamlStream yamlStreamRegion1 = parser.ParseYamlStream( inputRegion1, out successRegion1 );
                    //if ( !successRegion1 )
                    //{
                    //    yamlErrors++;
                    //}

                    //TextInput inputRegion2 = new TextInput( yamlPreprocessor( File.ReadAllText( region2Filename ) ) );
                    //YamlStream yamlStreamRegion2 = parser.ParseYamlStream( inputRegion2, out successRegion2 );
                    //if ( !successRegion2 )
                    //{
                    //    yamlErrors++;
                    //}

                    //TextInput inputLandmark = new TextInput( yamlPreprocessor( File.ReadAllText( landmarkFilename ) ) );
                    //YamlStream yamlStreamLandmark = parser.ParseYamlStream( inputLandmark, out successLandmark );
                    //if ( !successLandmark )
                    //{
                    //    yamlErrors++;
                    //}

                    //TextInput inputLandmark2 = new TextInput( yamlPreprocessor( File.ReadAllText( landmark2Filename ) ) );
                    //YamlStream yamlStreamLandmark2 = parser.ParseYamlStream( inputLandmark2, out successLandmark2 );
                    //if ( !successLandmark2 )
                    //{
                    //    yamlErrors++;
                    //}

                    //TextInput inputContour;
                    //YamlStream yamlStreamContour = null;
                    //if ( contourFileFound )
                    //{
                    //    inputContour = new TextInput( yamlPreprocessor( File.ReadAllText( contourFilename ) ) );
                    //    yamlStreamContour = parser.ParseYamlStream( inputContour, out successContour );
                    //}

                    if ( successRegion1 || successRegion2 || successLandmark || successLandmark2 || successContour )
                    {
                        // Create a container for the particular features which are labelled in the YAML
                        List<ILabelledFeature> features = new List<ILabelledFeature>();

                        //if ( successRegion1 )
                        //{
                        //    // features.AddRange( extractRegionsFromYaml( yamlStreamRegion1 ) );
                        //}
                        //if ( successRegion2 )
                        //{
                        //    features.AddRange( extractRegionsFromYaml( yamlStreamRegion2 ) );
                        //}
                        //if ( successLandmark )
                        //{
                        //    // features.AddRange( extractPointsFromYaml( yamlStreamLandmark ) );
                        //}
                        //if ( successLandmark2 )
                        //{
                        //    features.AddRange( extractPointsFromYaml( yamlStreamLandmark2 ) );
                        //}
                        //if ( successContour )
                        //{
                        //    features.AddRange( extractContoursFromYaml( yamlStreamContour ) );
                        //}

                        // This frame has the two eye features from above, plus the regions as well
                        IMediaFrame mediaFrame = new LazyLoadImageMediaFrame( imageFilename, features );

                        // This mediaFrame sequence only has the one frame in it
                        mediaSequence.Add( new SingleFrameMediaSequence( getName() + "_subject" + subject.ToString( "000" ), mediaFrame ) );
                    }

                }
            }
            Assert.LessOrEqual( missingFiles, 1, "More than expected number of image files were not found (expected 1)" );
            Assert.LessOrEqual( yamlErrors, 0, "More than expected number of YAML errors occurred (expected 0)" );
            Assert.AreEqual( 2182, contourFiles, "Wrong number of contour files found (expected 2182, although doco says 2193)" );
            Assert.LessOrEqual( regionAllZero, 24457, "More than expected number of regionAllZero occurred (expected 24468)" );
            Assert.LessOrEqual( notFoundRegionCorners, 0, "More than expected number of notFoundRegionCorners occurred (expected 0)" );
            Assert.LessOrEqual( unrecognisedFeatureName, 0, "More than expected number of unrecognisedFeatureName occurred (expected 0)" );
            return mediaSequence;
        }

        private IEnumerable<ILabelledFeature> extractRegionsFromYaml( YamlStream yamlStream )
        {
            List<ILabelledFeature> features = new List<ILabelledFeature>();

            // Pull out the regions from the YAML files
            foreach ( YamlDocument doc in yamlStream.Documents )
            {
                Assert.IsInstanceOf( typeof( Mapping ), doc.Root );
                Mapping m = ( Mapping ) doc.Root;
                foreach ( MappingEntry me in m.Enties )
                {
                    Assert.IsInstanceOf( typeof( Mapping ), me.Value );
                    Mapping mPoints = ( Mapping ) me.Value;

                    Dictionary<String, Int32> coords = MappingConverter.convertToDictionary( mPoints, MappingConverter.KeyCase.FORCE_LOWER );
                    Point[] points = new Point[ 4 ];
                    bool found = true;
                    int x = 0;
                    int y = 0;
                    int width = 0;
                    int height = 0;
                    found = found && coords.TryGetValue( "x", out x );
                    found = found && coords.TryGetValue( "y", out y );
                    found = found && coords.TryGetValue( "width", out width );
                    found = found && coords.TryGetValue( "height", out height );
                    if ( found )
                    {
                        if ( ( x == 0 ) && ( y == 0 ) && ( width == 0 ) && ( height == 0 ) )
                        {
                            regionAllZero++;
                        }
                        else
                        {
                            points[ 0 ] = new Point( x, y );
                            points[ 1 ] = new Point( x + width, y );
                            points[ 2 ] = new Point( x + width, y + height );
                            points[ 3 ] = new Point( x, y + height );
                            DataSetEnums.FeatureName featureName;
                            //bool featureNameFound = tryGetFeatureNameFor( me.Key.ToString(), out featureName );
                            //if ( featureNameFound )
                            //{
                            //    features.Add( new LabelledFeature( featureName, new MultiPointPath( points, true ) ) );
                            //}
                            //else
                            //{
                            //    unrecognisedFeatureName++;
                            //}
                        }
                    }
                    else
                    {
                        notFoundRegionCorners++;
                    }
                }
            }
            return features;
        }

        private IEnumerable<ILabelledFeature> extractPointsFromYaml( YamlStream yamlStream )
        {
            List<ILabelledFeature> features = new List<ILabelledFeature>();

            // Pull out the locations from the YAML files
            foreach ( YamlDocument doc in yamlStream.Documents )
            {
                Assert.IsInstanceOf( typeof( Mapping ), doc.Root );
                Mapping m = ( Mapping ) doc.Root;
                foreach ( MappingEntry me in m.Enties )
                {
                    Assert.IsInstanceOf( typeof( Mapping ), me.Value );
                    Mapping mPoints = ( Mapping ) me.Value;

                    Dictionary<String, Int32> coords = MappingConverter.convertToDictionary( mPoints, MappingConverter.KeyCase.FORCE_LOWER );
                    Point[] points = new Point[ 1 ];
                    bool found = true;
                    int x = 0;
                    int y = 0;
                    found = found && coords.TryGetValue( "x", out x );
                    found = found && coords.TryGetValue( "y", out y );
                    if ( found )
                    {
                        if ( ( x == 0 ) && ( y == 0 ) )
                        {
                            regionAllZero++;
                        }
                        else
                        {
                            points[ 0 ] = new Point( x, y );
                            DataSetEnums.FeatureName featureName;
                            //bool featureNameFound = tryGetFeatureNameFor( me.Key.ToString(), out featureName );
                            //if ( featureNameFound )
                            //{
                            //    features.Add( new LabelledFeature( featureName, new MultiPointPath( points, true ) ) );
                            //}
                            //else
                            //{
                            //    unrecognisedFeatureName++;
                            //}
                        }
                    }
                    else
                    {
                        notFoundRegionCorners++;
                    }
                }
            }
            return features;
        }

        private IEnumerable<ILabelledFeature> extractContoursFromYaml( YamlStream yamlStream )
        {
            List<ILabelledFeature> features = new List<ILabelledFeature>();
            //return features;
            // Pull out the contours from the YAML files
            foreach ( YamlDocument doc in yamlStream.Documents )
            {
                Assert.IsInstanceOf( typeof( Mapping ), doc.Root );
                Mapping m = ( Mapping ) doc.Root;

                // Validate that this refers to the correct file 'Image file' should be '00011010.jpg' for example
                Assert.True( m.Enties[ 0 ].Key.ToString().CompareTo( "Image file" ) == 0, "Unexpected order of Mapping elements" );
                Assert.True( m.Enties[ 1 ].Key.ToString().CompareTo( "Contours count" ) == 0, "Unexpected order of Mapping elements" );
                Assert.True( m.Enties[ 2 ].Key.ToString().CompareTo( "Contours" ) == 0, "Unexpected order of Mapping elements" );
                // m.Enties[ 2 ].Value;

                Assert.IsInstanceOf( typeof( Sequence ), m.Enties[ 2 ].Value );
                Sequence seqContours = ( Sequence ) m.Enties[ 2 ].Value;

                foreach ( DataItem di in seqContours.Enties )
                {
                    Assert.IsInstanceOf( typeof( Mapping ), di );
                    Mapping mdi = ( Mapping ) di;
                    Assert.True( mdi.Enties[ 0 ].Key.ToString().CompareTo( "Name" ) == 0, "Unexpected order of Mapping elements" );
                    Assert.True( mdi.Enties[ 1 ].Key.ToString().CompareTo( "Count" ) == 0, "Unexpected order of Mapping elements" );
                    Assert.True( mdi.Enties[ 2 ].Key.ToString().CompareTo( "Closed" ) == 0, "Unexpected order of Mapping elements" );
                    Assert.True( mdi.Enties[ 3 ].Key.ToString().CompareTo( "Points" ) == 0, "Unexpected order of Mapping elements" );

                    DataSetEnums.FeatureName featureName;
                    //bool featureNameFound = tryGetFeatureNameFor( mdi.Enties[ 0 ].Value.ToString(), out featureName );

                    //// Skip this one if unrecognised feature name
                    //if ( !featureNameFound )
                    //{
                    //    unrecognisedFeatureName++;
                    //    continue;
                    //}

                    // Skip this one if there is a zero 'Contours count'
                    if ( Convert.ToInt32( mdi.Enties[ 1 ].Value.ToString() ) == 0 )
                    {
                        noContours++;
                        continue;
                    }

                    // Figure out if closed or open
                    bool closed = ( mdi.Enties[ 2 ].Value.ToString().CompareTo( "1" ) == 0 );

                    // Now extract the sequence of x,y values
                    Sequence pointSeq = ( Sequence ) mdi.Enties[ 3 ].Value;
                    Point[] points = new Point[ pointSeq.Enties.Count ];
                    for ( int i = 0; i < pointSeq.Enties.Count; i++ )
                    {
                        Mapping pointMap = ( Mapping ) pointSeq.Enties[ i ];
                        Dictionary<String, Int32> coords = MappingConverter.convertToDictionary( pointMap, MappingConverter.KeyCase.FORCE_LOWER );
                        bool found = true;
                        int x = 0;
                        int y = 0;
                        found = found && coords.TryGetValue( "x", out x );
                        found = found && coords.TryGetValue( "y", out y );
                        Assert.True( found, "Could not convert coordinates" );
                        points[ i ] = new Point( x, y );
                    }
                    // This feature is a multiple point path
                    // features.Add( new LabelledFeature( featureName, new MultiPointPath( points, closed ) ) );
                }
            }
            return features;
        }

    }
}
