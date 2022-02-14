/* Copyright (C) 2018 Kevin Boronka
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace HttpPack
{
    public class HttpController
    {
        private readonly Dictionary<string, MethodInfo> actions;

        private readonly Type type;

        public HttpController(Type controller)
        {
            type = controller;
            actions = new Dictionary<string, MethodInfo>();

            foreach (var obj in controller.GetCustomAttributes(false))
            {
                if (obj is IsPrimaryController)
                {
                    Primary = this;
                }
            }

            foreach (var method in controller.GetMethods())
            {
                if (!method.IsSpecialName && method.IsStatic && method.IsPublic &&
                    method.ReturnType == typeof(HttpContent) && method.GetParameters().Length == 1 &&
                    method.GetParameters()[0].ParameterType == typeof(HttpRequest))
                {
                    actions.Add(method.Name, method);
                    foreach (var obj in method.GetCustomAttributes(false))
                    {
                        if (obj is IsPrimaryAction)
                        {
                            PrimaryAction = method;
                        }

                        if (obj is ViewAlias)
                        {
                            actions.Add(((ViewAlias) obj).Alias, method);
                        }
                    }
                }
            }
        }

        public string FullName => type.FullName;

        public string Name => type.Name;

        public MethodInfo PrimaryAction { get; }

        #region static members and methods

        private static Dictionary<string, HttpController> controllers;

        public static HttpController Primary { get; private set; }

        public static void LoadControllers(List<Assembly> assemblies)
        {
            controllers = new Dictionary<string, HttpController>();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.Name.EndsWith("Controller"))
                    {
                        foreach (var attribute in type.GetCustomAttributes(false))
                        {
                            if (attribute is IsController)
                            {
                                // add the sar controller
                                var controllerName = type.Name.Substring(0, type.Name.Length - "Controller".Length);
                                controllers.Add(controllerName, new HttpController(type));
                            }
                        }
                    }
                }
            }
        }

        public static bool ActionExists(HttpRequest request)
        {
            var urlSplit = request.Path.Split('/');

            if (urlSplit.Length != 2)
            {
                return false;
            }

            var controllerName = urlSplit[0];
            var actionName = urlSplit[1];
            if (!controllers.ContainsKey(controllerName))
            {
                return false;
            }

            if (!controllers[controllerName].actions.ContainsKey(actionName))
            {
                return false;
            }

            return true;
        }

        public static HttpContent RequestPrimary(HttpRequest request)
        {
            var contentObject = Primary.PrimaryAction.Invoke(null, new object[] {request});
            return (HttpContent) contentObject;
        }

        public static HttpContent RequestAction(HttpRequest request)
        {
            var urlSplit = request.Path.Split('/');
            var controllerName = urlSplit[0];
            var actionName = urlSplit[1];

            return RequestAction(controllerName, actionName, request);
        }

        public static HttpContent RequestAction(string controllerName, string actionName, HttpRequest request)
        {
            if (!controllers.ContainsKey(controllerName))
            {
                throw new FileNotFoundException("controller " + @"""" + controllerName + @"""" + " not found");
            }

            var controller = controllers[controllerName];

            if (!controller.actions.ContainsKey(actionName))
            {
                throw new FileNotFoundException("action " + @"""" + actionName + @"""" + " not found in controller " +
                                                @"""" + controllerName + @"""");
            }

            var action = controller.actions[actionName];

            var contentObject = action.Invoke(null, new object[] {request});
            return (HttpContent) contentObject;
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class IsPrimaryAction : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ViewAlias : Attribute
    {
        public ViewAlias(string alias)
        {
            Alias = alias;
        }

        public string Alias { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class IsPrimaryController : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class IsController : Attribute
    {
    }
}