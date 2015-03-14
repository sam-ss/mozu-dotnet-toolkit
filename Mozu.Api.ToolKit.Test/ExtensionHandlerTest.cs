using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mozu.Api.ToolKit.Handlers;
using Mozu.Api.ToolKit.Models;

namespace Mozu.Api.ToolKit.Test
{
    [TestClass]
    public class ExtensionHandlerTest : BaseTest
    {
        private ISubnavLinkHandler _subnavLinkHandler;
        private string _appUrl = "https://f6b273a766009bfb.a.passageway.io/app";

        [TestInitialize]
        public void Initialize()
        {
            _subnavLinkHandler = Container.Resolve<ISubnavLinkHandler>();
        }

        [TestMethod]
        public void AddOrdersLinks()
        {
            var subNavlink = new SubnavLink
            {
                ParentId = Parent.Orders,
                Href = String.Format("{0}/orders",_appUrl),
                Path = new []{ "MyApp", "Orders" },
                WindowTitle = "My Extension App Orders"
            };
            _subnavLinkHandler.AddUpdateExtensionLinkAsync(TenantId, subNavlink).Wait();
        }


        [TestMethod]
        public void AddProductsLinks()
        {
            var subNavlink = new SubnavLink
            {
                ParentId = Parent.Catalog,
                Href = String.Format("{0}/products", _appUrl),
                Path = new[] { "MyApp", "Products" },
                WindowTitle = "My Extension App Products"
            };
            _subnavLinkHandler.AddUpdateExtensionLinkAsync(TenantId, subNavlink).Wait();
        }


        [TestMethod]
        public void AddContactsLinks()
        {
            var subNavlink = new SubnavLink
            {
                ParentId = Parent.Customers,
                Href = String.Format("{0}/contacts", _appUrl),
                Path = new[] { "MyApp", "Contacts" },
                WindowTitle = "My Extension App Contacts"
            };
            _subnavLinkHandler.AddUpdateExtensionLinkAsync(TenantId, subNavlink).Wait();
        }

        [TestMethod]
        public void DeleteLinksTest()
        {
            _subnavLinkHandler.Delete(TenantId).Wait();
        }

    }
}
