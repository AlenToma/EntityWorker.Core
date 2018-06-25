using EntityWorker.Core.Attributes;
using EntityWorker.Core.FastDeepCloner;
using EntityWorker.Core.Helper;
using EntityWorker.Core.InterFace;
using EntityWorker.Core.SqlQuerys;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace EntityWorker.Core.Object.Library.XML
{
    internal static class XmlUtility
    {
        /// <summary>
        /// serialize object to xml
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string ToXml(this object o)
        {
            string xmlResult = "";
            ToXml(o, ref xmlResult);
            var xmlDoc = new System.Xml.XmlDocument();
            StringWriter sw = new StringWriter();
            xmlDoc.LoadXml(xmlResult);
            xmlDoc.Save(sw);
            return sw.ToString();
        }

        /// <summary>
        /// DeSerilize Xml to IAnimal, this is supposed to handle all unknow object types but there has not been to many tests.
        /// Only IAnimal test been done here.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static object FromXml(this string xml, IRepository transaction = null)  
        {
            if (string.IsNullOrEmpty(xml))
                return null;
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);
            var o = FromXml(doc.DocumentElement);
            void LoadXmlIgnoreProperties(object item)
            {
                if (item is IList)
                {
                    foreach (var t in (IList)item)
                        LoadXmlIgnoreProperties(t);
                    return;
                }

                var type = item?.GetType().GetActualType();
                if (type == null)
                    return;
                if (!(item?.GetPrimaryKeyValue().ObjectIsNew() ?? true))
                {
                    var primaryId = item.GetPrimaryKeyValue();
                    foreach (var prop in DeepCloner.GetFastDeepClonerProperties(item.GetType()).Where(x => (x.ContainAttribute<XmlIgnore>() || !x.IsInternalType) && !x.ContainAttribute<ExcludeFromAbstract>() && x.CanRead))
                    {
                        var value = prop.GetValue(item);
                        if (prop.PropertyType == typeof(string) && string.IsNullOrEmpty(value?.ToString()))
                            value = string.Empty;
                        if (prop.IsInternalType && value == LightDataTableShared.ValueByType(prop.PropertyType)) // Value is default
                        {
                            var cmd = transaction.GetSqlCommand($"SELECT [{prop.GetPropertyName()}] FROM {type.TableName().GetName(transaction.DataBaseTypes)} WHERE [{item.GetPrimaryKey().GetPropertyName()}] = {Querys.GetValueByType(item.GetPrimaryKeyValue(), transaction.DataBaseTypes)}");
                            var data = transaction.ExecuteScalar(cmd);
                            if (data == null)
                                continue;
                            if (prop.ContainAttribute<DataEncode>())
                                data = new DataCipher(prop.GetCustomAttribute<DataEncode>().Key, prop.GetCustomAttribute<DataEncode>().KeySize).Decrypt(data.ToString());
                            else if (prop.ContainAttribute<ToBase64String>() && data.ToString().IsBase64String())
                                data = MethodHelper.DecodeStringFromBase64(data.ToString());

                            prop.SetValue(item, data.ConvertValue(prop.PropertyType));
                        }
                        else if (value != null) LoadXmlIgnoreProperties(value);
                    }

                }
            }
            if (transaction != null)
                LoadXmlIgnoreProperties(o);
            return o;

        }

        /// <summary>
        /// DeSerilize Xml to IAnimal, this is supposed to handle all unknow object types but there has not been to many tests.
        /// Only IAnimal test been done here.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static T FromXml<T>(this string xml, IRepository transaction) where T : class
        {
            if (string.IsNullOrEmpty(xml))
                return (T)(new object());
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);
            var o = (T)FromXml(doc.DocumentElement);
            void LoadXmlIgnoreProperties(object item)
            {
                if (item is IList)
                {
                    foreach (var t in (IList)item)
                        LoadXmlIgnoreProperties(t);
                    return;
                }

                var type = item?.GetType().GetActualType();
                if (type == null)
                    return;
                if (!(item?.GetPrimaryKeyValue().ObjectIsNew() ?? true))
                {
                    var primaryId = item.GetPrimaryKeyValue();
                    foreach (var prop in DeepCloner.GetFastDeepClonerProperties(item.GetType()).Where(x => (x.ContainAttribute<XmlIgnore>() || !x.IsInternalType) && !x.ContainAttribute<ExcludeFromAbstract>() && x.CanRead))
                    {
                        var value = prop.GetValue(item);
                        if (prop.PropertyType == typeof(string) && string.IsNullOrEmpty(value?.ToString()))
                            value = string.Empty;
                        if (prop.IsInternalType && value == LightDataTableShared.ValueByType(prop.PropertyType)) // Value is default
                        {
                            var cmd = transaction.GetSqlCommand($"SELECT [{prop.GetPropertyName()}] FROM {type.TableName().GetName(transaction.DataBaseTypes)} WHERE [{item.GetPrimaryKey().GetPropertyName()}] = {Querys.GetValueByType(item.GetPrimaryKeyValue(), transaction.DataBaseTypes)}");
                            var data = transaction.ExecuteScalar(cmd);
                            if (data == null)
                                continue;
                            if (prop.ContainAttribute<DataEncode>())
                                data = new DataCipher(prop.GetCustomAttribute<DataEncode>().Key, prop.GetCustomAttribute<DataEncode>().KeySize).Decrypt(data.ToString());
                            else if (prop.ContainAttribute<ToBase64String>() && data.ToString().IsBase64String())
                                data = MethodHelper.DecodeStringFromBase64(data.ToString());

                            prop.SetValue(item, data.ConvertValue(prop.PropertyType));
                        }
                        else if (value != null) LoadXmlIgnoreProperties(value);
                    }

                }
            }
            if (transaction != null)
                LoadXmlIgnoreProperties(o);
            return o;

        }


        private static object FromXml(XmlNode doc)
        {
            var fullType = Type.GetType(doc.Attributes["FullName"].Value);
            object item = fullType.CreateInstance();
            var subtype = fullType.GetActualType();
            foreach (XmlNode chil in doc.ChildNodes)
            {
                var prop = DeepCloner.GetProperty(subtype, chil.Name);
                if (chil.Attributes["FullName"] != null && prop == null)
                {
                    if (chil.Name != "List")
                        ((IList)item).Add(FromXml(chil));

                    else foreach (var t in FromXml(chil) as IList)
                            ((IList)item).Add(t);
                    continue;
                }
                if (prop != null && prop.IsInternalType)
                    prop.SetValue(item, chil.InnerText.ConvertValue(prop.PropertyType));
                else if (chil.Attributes["FullName"] != null)
                    prop.SetValue(item, FromXml(chil));
                else ((IList)item).Add(chil.InnerText.ConvertValue(item.GetType().GetActualType()));

            }
            return item;
        }


        private static string ToXml(object o, ref string xml, bool addStart = true, string propName = "")
        {
            if (addStart)
                xml += "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            var name = string.IsNullOrWhiteSpace(propName) ? new Regex("[^a-zA-Z]").Replace(o.GetType().Name, "") : propName;
            var quName = o.GetType().AssemblyQualifiedName;

            if (o.GetType().IsInternalType())
            {
                if (o is byte[])
                    o = Convert.ToBase64String(o as byte[]);
                xml += $"<{name}>{o}</{name}>";
                return xml;
            }

            xml += $"<{name} FullName=\"{quName}\">";

            if (o is IList)
            {
                foreach (var item in (IList)o)
                    ToXml(item, ref xml, false);
            }
            else
            {
                foreach (var prop in DeepCloner.GetFastDeepClonerProperties(o.GetType()).Where(x => !x.ContainAttribute<XmlIgnore>()))
                {
                    var value = prop.GetValue(o);
                    if (value == null)
                        continue;
                    var zName = prop.Name;

                    if (value is byte[])
                        value = Convert.ToBase64String(value as byte[]);
                    if (value.GetType().IsInternalType())
                        xml += $"<{zName}>{value}</{zName}>";
                    else ToXml(value, ref xml, false, zName);
                }
            }

            xml += $"</{name}>";
            return xml;

        }
    }
}
