using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SlapCrop.Tests.Mocks
{
    public class TestImageRunner : ImageProcessRunner
    {
        public TestImageRunner(HttpContextBase context)
            : base(context)
        {
        }

        protected override Bitmap GetSourceImage()
        {
            var filename = Path.GetFileName(this.Context.Request.Path);
            try
            {
                return ResHelper.LoadImage(filename);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
