// using System.Xml.Serialization;
// XmlSerializer serializer = new XmlSerializer(typeof(Profile));
// using (StringReader reader = new StringReader(xml))
// {
//    var test = (Profile)serializer.Deserialize(reader);
// }

using System.Xml.Serialization;

[XmlRoot(ElementName="InternalID")]
public class InternalID { 

	[XmlAttribute(AttributeName="nil")] 
	public bool Nil { get; set; } 
}