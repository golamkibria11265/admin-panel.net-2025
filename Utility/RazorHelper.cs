using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Utility
{
    public class RazorHelper
    {
        public static string RenderRazorViewToString(Controller controller, string viewName, object? model = null)
        {
            string result = string.Empty;
            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                IViewEngine? viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                if (viewEngine != null)
                {
                    ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, false);
                    if (viewResult.View != null)
                    {
                        ViewContext viewContext = new(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw,
                        new HtmlHelperOptions());
                        viewResult.View.RenderAsync(viewContext);
                        result = sw.GetStringBuilder().ToString();
                    }
                }
                return result;
            }
        }
    }
}
