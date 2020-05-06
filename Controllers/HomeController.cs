using PMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PMS.Controllers
{
    public class HomeController : Controller
    {
        private PMS_EF db = new PMS_EF();
        
        public string type = "";
        public string option = "";

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection fc)
        {
            string type = fc["type"];//获取点击的是登录（管理员/业主/租户）还是注册
            if (type == "Register")
            {
                if (fc["loginacct"] == "" || fc["password"] == "" || fc["password1"] == "" || fc["accounttype"] == "")
                {
                    return Content("<script>alert('注册账号时请填写全部信息');history.go(-1);</script>");
                }
                else if (fc["password1"] != fc["password"])
                {
                    return Content("<script>alert('密码与确认密码不一致！');history.go(-1);</script>");
                }
                else
                {
                    string accounttype = fc["accounttype"];
                    string loginacct = fc["loginacct"];
                    string password = fc["password"];

                    if (accounttype == "type1")//系统管理员
                    {
                        var exist = db.propertycompany.FirstOrDefault(b => b.Pacct == loginacct);
                        if (exist != null)
                        {
                            return Content("<script>alert('该账号已存在，请重新输入');history.go(-1);</script>");
                        }
                        else
                        {
                            var newpropertycompany = new propertycompany()
                            {
                                Pacct = loginacct,
                                Ppass = password
                            };
                            db.propertycompany.Add(newpropertycompany);
                            db.SaveChanges();
                            return Content("<script>alert('注册成功！');history.go(-1);</script>");
                        }
                        
                    }
                    else if (accounttype == "type2")//业主
                    {
                        var exist = db.owner.FirstOrDefault(b => b.Oacct == loginacct);
                        if (exist != null)
                        {
                            return Content("<script>alert('该账号已存在，请重新输入');history.go(-1);</script>");
                        }
                        else
                        {
                            var newowner = new owner()
                            {
                                Oacct = loginacct,
                                Opass = password
                            };
                            db.owner.Add(newowner);
                            db.SaveChanges();
                            return Content("<script>alert('注册成功！');history.go(-1);</script>");
                        }
                    }
                    else if (accounttype == "type3")//租户
                    {

                        var exist = db.tenant.FirstOrDefault(b => b.Tacct == loginacct);
                        if (exist != null)
                        {
                            return Content("<script>alert('该账号已存在，请重新输入');history.go(-1);</script>");
                        }
                        else
                        {
                            var newtenant = new tenant()
                            {
                                Tacct = loginacct,
                                Tpass = password
                            };
                            db.tenant.Add(newtenant);
                            db.SaveChanges();
                            return Content("<script>alert('注册成功！');history.go(-1);</script>");
                        }
                    }
                    else
                    {
                        return Content("<script>alert('请选择账号类型');history.go(-1);</script>");
                    }
                }
            }
            else//登录
            {
                if (type == "Manager")
                {
                    if (fc["loginacct"] == "" || fc["password"] == "")
                    {
                        return Content("<script>alert('请输入账号密码');history.go(-1);</script>");
                    }
                    else
                    {
                        string loginacct = fc["loginacct"];
                        string password = fc["password"];

                        var com = db.propertycompany.Where(b => b.Pacct == loginacct & b.Ppass == password);
                        if (com.Count() > 0)
                        {
                            //用于当前页面参数传递，前台用<input id="taskType" name="taskType" type="hidden" value='@ViewBag.taskType' />取出
                            //ViewBag.displayname = loginacct;

                            //跳转不同界面的参数传递：
                            //return RedirectToAction(_cViewPage, new {taskType=taskType });
                            //<input id="taskType" name="taskType" type="hidden" value='@ViewData["taskType"]' />
                            //或者<input id="taskType" name="taskType" type="hidden" value='@Request.Params["taskType"]' />

                            Session.Add("user", com);


                            db.Dispose();

                            return RedirectToAction("Index", "PropertyCompany", new { displayname = loginacct });

                        }
                        else
                        {
                            db.Dispose();
                            return Content("<script>alert('账号或密码错误!');history.go(-1);</script>");
                        }

                    }
                    

                }
                else if (type == "Owner")
                {
                    if (fc["loginacct"] == "" || fc["password"] == "")
                    {
                        return Content("<script>alert('请输入账号密码');history.go(-1);</script>");
                    }
                    else
                    {
                        string loginacct = fc["loginacct"];
                        string password = fc["password"];

                        var com = db.owner.Where(b => b.Oacct == loginacct & b.Opass == password);
                        if (com.Count() > 0)
                        {
                            var oname = com.Select(b => b.Oname).FirstOrDefault();
                            if (oname == null)
                            {
                                Session["tocomplete"] = "";
                               // ViewBag.tocomplete = "";
                                Session["loginacct"] = loginacct;
                                Session["password"] = password;
                            }
                            else
                            {
                                Session["tocomplete"] = "display:none";
                               // ViewBag.tocomplete = "display:none";
                            }
                            Session.Add("user", com);
                            db.Dispose();
                            return RedirectToAction("Index", "Owner", new { displayname = oname });
                        }
                        else
                        {
                            db.Dispose();
                            return Content("<script>alert('账号或密码错误!');history.go(-1);</script>");
                            //return Content("<script>lightyear.notify('账号或密码错误！', 'success', 5000, 'mdi mdi-emoticon-happy', 'top', 'center');</script>");
                        }

                    }
                }
                else if (type == "Tenant")
                {
                    if (fc["loginacct"] == "" || fc["password"] == "")
                    {
                        return Content("<script>alert('请输入账号密码');history.go(-1);</script>");
                    }
                    else
                    {
                        string loginacct = fc["loginacct"];
                        string password = fc["password"];

                        var com = db.tenant.Where(b => b.Tacct == loginacct & b.Tpass == password);
                        if (com.Count() > 0)
                        {
                            var tname = com.Select(b => b.Tname).FirstOrDefault();
                            if (tname == null)
                            {
                                Session["tocomplete"] = "";
                               // ViewBag.tocomplete = "";
                                Session["loginacct"] = loginacct;
                                Session["password"] = password;
                            }
                            else
                            {
                                Session["tocomplete"] = "display:none";
                               // ViewBag.tocomplete = "display:none";
                            }
                            Session.Add("user", com);
                            db.Dispose();
                            return RedirectToAction("Index", "Tenant", new { displayname = tname });
                        }
                        else
                        {
                            db.Dispose();
                            return Content("<script>alert('账号或密码错误!');history.go(-1);</script>");
                        }

                    }
                }
                else
                {
                    return Content("<script>alert('发生未知的错误>_<');history.go(-1);</script>");
                }
                
            }
            
        }

        //忘记密码操作
        [HttpPost]
        public ActionResult forgetpassword(FormCollection fc)
        {

            if (fc["type"] == "" || fc["code"] == "" || fc["acct"] == "" || fc["pass"] == "" || fc["pass1"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else if (fc["pass"] != fc["pass1"])
            {
                return Content("<script>alert('前后密码不一致');history.go(-1);</script>");
            }
            else
            {
                //获取表单数据
                string type = fc["type"];
                int id = Convert.ToInt32(fc["code"]);
                string acct = fc["acct"];
                string pass = fc["pass"];

                if (type == "物业系统管理员")
                {
                    var admin = db.propertycompany.FirstOrDefault(a => a.Pid == id && a.Pacct == acct);
                    if (admin != null)
                    {
                        admin.Ppass = pass;
                        db.SaveChanges();
                        return Content("<script>alert('修改成功');window.location.href='/Home/Index';</script>");
                    }
                    else
                    {
                        return Content("<script>alert('编码或账号输入有误');history.go(-1);</script>");
                    }
                }
                else if (type == "业主")
                {
                    var owner = db.owner.FirstOrDefault(o=>o.Oid==id&&o.Oacct==acct);
                    if (owner != null)
                    {
                        owner.Opass = pass;db.SaveChanges();
                    return Content("<script>alert('修改成功');window.location.href='/Home/Index';</script>");
                    }
                    else
                    {
                        return Content("<script>alert('编码或账号输入有误');history.go(-1);</script>");
                    }
                }
                else
                {
                    var tenant = db.tenant.FirstOrDefault(a => a.Tid == id && a.Tacct == acct);
                    if (tenant != null)
                    {
                        tenant.Tpass = pass;db.SaveChanges();
                    return Content("<script>alert('修改成功');window.location.href='/Home/Index';</script>");
                    }
                    else
                    {
                        return Content("<script>alert('编码或账号输入有误');history.go(-1);</script>");
                    }
                }
            }
        }






        //跳转忘记密码
        public ActionResult forgetpassword()
        {
            return View("forgetpassword");
        }



        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}