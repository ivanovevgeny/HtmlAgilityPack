using System.IO;
using System.Linq;
using System.Net;
using NUnit.Framework;

namespace HtmlAgilityPack.Tests
{
	[TestFixture]
	public class HtmlDocumentTests
	{
		private string _contentDirectory;
        private HtmlDocument _doc;
	

		[SetUp]
		public void Setup()
		{
			_contentDirectory = Directory.GetCurrentDirectory() + "\\files\\";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true; // иначе не работает загрузка некоторых https
		}

		private HtmlDocument GetMshomeDocument()
		{
			var doc = new HtmlDocument();
			doc.Load(_contentDirectory + "mshome.htm");
			return doc;
		}

		[Test]
		public void StackOverflow()
		{
			var url = "http://rewarding.me/active-tel-domains/index.php/index.php?rescan=amour.tel&w=A&url=&by=us&limits=0";
			var request = WebRequest.Create(url);
			var htmlDocument = new HtmlDocument();
			htmlDocument.Load((request.GetResponse()).GetResponseStream());
			Stream memoryStream = new MemoryStream();
			htmlDocument.Save(memoryStream);
		}
		[Test]
		public void CreateAttribute()
		{
			var doc = new HtmlDocument();
			var a = doc.CreateAttribute("href");
			Assert.AreEqual("href", a.Name);
		}

		[Test]
		public void CreateAttributeWithEncodedText()
		{
			var doc = new HtmlDocument();
			var a = doc.CreateAttribute("href", "http://something.com\"&<>");
			Assert.AreEqual("href", a.Name);
			Assert.AreEqual("http://something.com\"&<>", a.Value);
		}

		[Test]
		public void CreateAttributeWithText()
		{
			var doc = new HtmlDocument();
			var a = doc.CreateAttribute("href", "http://something.com");
			Assert.AreEqual("href", a.Name);
			Assert.AreEqual("http://something.com", a.Value);
		}

		//[Test]
		//public void CreateComment()
		//{
		//    var doc = new HtmlDocument();
		//    var a = doc.CreateComment();
		//    Assert.AreEqual(HtmlNode.HtmlNodeTypeNameComment, a.Name);
		//    Assert.AreEqual(a.NodeType, HtmlNodeType.Comment);
		//}

		//[Test]
		//public void CreateCommentWithText()
		//{
		//    var doc = new HtmlDocument();
		//    var a = doc.CreateComment("something");
		//    Assert.AreEqual(HtmlNode.HtmlNodeTypeNameComment, a.Name);
		//    Assert.AreEqual("something", a.InnerText);
		//    Assert.AreEqual(a.NodeType, HtmlNodeType.Comment);
		//}

		[Test]
		public void CreateElement()
		{
			var doc = new HtmlDocument();
			var a = doc.CreateElement("a");
			Assert.AreEqual("a", a.Name);
			Assert.AreEqual(a.NodeType, HtmlNodeType.Element);
		}

		//[Test]
		//public void CreateTextNode()
		//{
		//    var doc = new HtmlDocument();
		//    var a = doc.CreateTextNode();
		//    Assert.AreEqual(HtmlNode.HtmlNodeTypeNameText, a.Name);
		//    Assert.AreEqual(a.NodeType, HtmlNodeType.Text);
		//}

		[Test]
		public void CreateTextNodeWithText()
		{
			var doc = new HtmlDocument();
			var a = doc.CreateTextNode("something");
			Assert.AreEqual("something", a.InnerText);
			Assert.AreEqual(a.NodeType, HtmlNodeType.Text);
		}

		[Test]
		public void HtmlEncode()
		{
			var result = HtmlDocument.HtmlEncode("http://something.com\"&<>");
			Assert.AreEqual("http://something.com&quot;&amp;&lt;&gt;", result);
		}

		[Test]
		public void TestParse()
		{
			var doc = GetMshomeDocument();
			Assert.IsTrue(doc.DocumentNode.DescendantNodes().Count() > 0);
		}

		[Test]
		public void TestParseSaveParse()
		{
			var doc = GetMshomeDocument();
			var doc1desc =
				doc.DocumentNode.DescendantNodes().Where(x => !string.IsNullOrWhiteSpace(x.InnerText)).ToList();
			doc.Save(_contentDirectory + "testsaveparse.html");

			var doc2 = new HtmlDocument();
			doc2.Load(_contentDirectory + "testsaveparse.html");
			var doc2desc =
				doc2.DocumentNode.DescendantNodes().Where(x => !string.IsNullOrWhiteSpace(x.InnerText)).ToList();
			Assert.AreEqual(doc1desc.Count, doc2desc.Count);
			//for(var i=0; i< doc1desc.Count;i++)
			//{
			//    try
			//    {
			//        Assert.AreEqual(doc1desc[i].Name, doc2desc[i].Name);
			//    }catch(Exception e)
			//    {
			//        throw;
			//    }
			//}
		}

		[Test]
		public void TestRemoveUpdatesPreviousSibling()
		{
			var doc = GetMshomeDocument();
			var docDesc = doc.DocumentNode.DescendantNodes().ToList();
			var toRemove = docDesc[1200];
			var toRemovePrevSibling = toRemove.PreviousSibling;
			var toRemoveNextSibling = toRemove.NextSibling;
			toRemove.Remove();
			Assert.AreSame(toRemovePrevSibling, toRemoveNextSibling.PreviousSibling);
		}

		[Test]
		public void TestReplaceUpdatesSiblings()
		{
			var doc = GetMshomeDocument();
			var docDesc = doc.DocumentNode.DescendantNodes().ToList();
			var toReplace = docDesc[1200];
			var toReplacePrevSibling = toReplace.PreviousSibling;
			var toReplaceNextSibling = toReplace.NextSibling;
			var newNode = doc.CreateElement("tr");
			toReplace.ParentNode.ReplaceChild(newNode, toReplace);
			Assert.AreSame(toReplacePrevSibling, newNode.PreviousSibling);
			Assert.AreSame(toReplaceNextSibling, newNode.NextSibling);
		}

		[Test]
		public void TestInsertUpdateSiblings()
		{
			var doc = GetMshomeDocument();
			var newNode = doc.CreateElement("td");
			var toReplace = doc.DocumentNode.ChildNodes[2];
			var toReplacePrevSibling = toReplace.PreviousSibling;
			var toReplaceNextSibling = toReplace.NextSibling;
			doc.DocumentNode.ChildNodes.Insert(2, newNode);
			Assert.AreSame(newNode.NextSibling, toReplace);
			Assert.AreSame(newNode.PreviousSibling, toReplacePrevSibling);
			Assert.AreSame(toReplaceNextSibling, toReplace.NextSibling);
		}

        [Test]
        public void TestLoadLargeShouldReturnFlag()
        {
            var web = new HtmlWeb
            {
                PreRequest = OnPreRequest,
                PostResponse = OnAfterResponse,
                PreHandleDocument =  OnPreHandleDocument
            };
            web.Load("https://valeriya-ivanovo.ru/catalog/muzhskoe_1/futbolki_3/14005/");
            Assert.True(web.MaxDocumentSizeReached);
            Assert.AreEqual(web.StatusCode, HttpStatusCode.OK);
            Assert.True(string.IsNullOrEmpty(_doc.DocumentNode?.InnerHtml));
            Assert.False(string.IsNullOrEmpty(_doc.RawText));
        }

        [Test]
        public void TestLoadOrdinary_ShouldNotReturnFlag()
        {
            var web = new HtmlWeb
            {
                PreRequest = OnPreRequest,
                PostResponse = OnAfterResponse,
                PreHandleDocument =  OnPreHandleDocument
            };
            web.Load("https://www.wikipedia.org/");
            Assert.False(web.MaxDocumentSizeReached);
            Assert.AreEqual(web.StatusCode, HttpStatusCode.OK);
            Assert.False(string.IsNullOrEmpty(_doc.DocumentNode?.InnerHtml));
            Assert.False(string.IsNullOrEmpty(_doc.RawText));
        }

        private void OnAfterResponse(HttpWebRequest request, HttpWebResponse response)
        {
            var len = response.ContentLength;
        }

        private bool OnPreRequest(HttpWebRequest request)
        {
            request.Timeout = 100000;
            request.ReadWriteTimeout = 10000;
			return true;
        }

        private void OnPreHandleDocument(HtmlDocument document)
        {
            _doc = document;
        }
    }
}