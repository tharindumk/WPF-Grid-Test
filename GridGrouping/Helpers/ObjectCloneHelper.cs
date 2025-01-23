using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers
{
    public static class ObjectCloneHelper
    {
        private static Dictionary<string, Dictionary<string, Type>> cloneTypes = new Dictionary<string, Dictionary<string, Type>>();

        public static T Clone<T>(T sourceObject) where T : new()
        {
            T newObject = default(T);
            return Clone(ref newObject, sourceObject);
        }

        public static T Clone<T>(ref T targetObject, T sourceObject) where T : new()
        {
            //return sourceObject;


            //First we create an instance of this specific type.
            try
            {
                if (sourceObject == null)
                    return default(T);

                if (targetObject == null)
                    targetObject = (T)Activator.CreateInstance(sourceObject.GetType());
                else if (targetObject.GetType() != sourceObject.GetType())
                    return default(T);

                //We get the array of fields for the new type instance.
                FieldInfo[] fields = targetObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                int i = 0;

                foreach (FieldInfo fi in sourceObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    //We query if the fiels support the ICloneable interface.
                    //Type ICloneType = fi.FieldType.
                    //            GetInterface("ICloneable", true);

                    Type ICloneType = GetCommonType("ICloneable",sourceObject.GetType(), fi.FieldType);

                    if (ICloneType != null)
                    {
                        //Getting the ICloneable interface from the object.
                        ICloneable IClone = (ICloneable)fi.GetValue(sourceObject);

                        if (IClone != null)
                        {
                            //We use the clone method to set the new value to the field.
                            fields[i].SetValue(targetObject, IClone.Clone());
                        }
                    }
                    else
                    {
                        // If the field doesn't support the ICloneable 
                        // interface then just set it.
                        fields[i].SetValue(targetObject, fi.GetValue(sourceObject));
                    }

                    //Now we check if the object support the 
                    //IEnumerable interface, so if it does
                    //we need to enumerate all its items and check if 
                    //they support the ICloneable interface.
                    //Type IEnumerableType = fi.FieldType.GetInterface
                    //                ("IEnumerable", true);

                    Type IEnumerableType = GetCommonType("IEnumerable", sourceObject.GetType(), fi.FieldType);

                    if (IEnumerableType != null)
                    {
                        //Get the IEnumerable interface from the field.
                        IEnumerable IEnum = (IEnumerable)fi.GetValue(sourceObject);

                        //This version support the IList and the 
                        //IDictionary interfaces to iterate on collections.
                        
                        //Type IListType = fields[i].FieldType.GetInterface
                        //                    ("IList", true);

                        Type IListType = GetCommonType("IList",sourceObject.GetType(), fields[i].FieldType);

                        //Type IDicType = fields[i].FieldType.GetInterface
                        //                    ("IDictionary", true);

                        Type IDicType = GetCommonType("IDictionary",sourceObject.GetType(), fields[i].FieldType);

                        int j = 0;
                        if (IListType != null)
                        {
                            //Getting the IList interface.
                            IList list = (IList)fields[i].GetValue(targetObject);

                            if (list != null)
                            {
                                foreach (object obj in IEnum)
                                {
                                    //Checking to see if the current item 
                                    //support the ICloneable interface.
                                    if (obj != null)
                                    {
                                        //ICloneType = obj.GetType().
                                        //    GetInterface("ICloneable", true);

                                        ICloneType = GetCommonType("ICloneable",sourceObject.GetType(), obj.GetType());

                                        if (ICloneType != null)
                                        {
                                            //If it does support the ICloneable interface, 
                                            //we use it to set the clone of
                                            //the object in the list.
                                            ICloneable clone = (ICloneable)obj;

                                            list[j] = clone.Clone();
                                        }
                                    }

                                    //NOTE: If the item in the list is not 
                                    //support the ICloneable interface then in the 
                                    //cloned list this item will be the same 
                                    //item as in the original list
                                    //(as long as this type is a reference type).

                                    j++;
                                }
                            }
                        }
                        else if (IDicType != null)
                        {
                            //Getting the dictionary interface.
                            IDictionary dic = (IDictionary)fields[i].
                                                GetValue(targetObject);
                            j = 0;

                            foreach (DictionaryEntry de in IEnum)
                            {
                                //Checking to see if the item 
                                //support the ICloneable interface.
                                //ICloneType = de.Value.GetType().
                                //    GetInterface("ICloneable", true);

                                ICloneType = GetCommonType("ICloneable",sourceObject.GetType(), de.Value.GetType());

                                if (ICloneType != null)
                                {
                                    ICloneable clone = (ICloneable)de.Value;

                                    dic[de.Key] = clone.Clone();
                                }
                                j++;
                            }
                        }
                    }
                    i++;
                }

                return targetObject;
            }
            catch (System.Exception ex)
            {
                ExceptionsLogger.LogError(ex);
                return default(T);
            }
        }

        private static Type GetCommonType(string interfaceName, Type classType, Type objectType)
        {
            if (cloneTypes.ContainsKey(interfaceName))
            {
                if (cloneTypes[interfaceName].ContainsKey(objectType.FullName))
                    return cloneTypes[interfaceName][objectType.FullName];

                Type ICloneType = objectType.GetInterface(interfaceName, true);

                if (ICloneType != null)
                {
                    if (!cloneTypes[interfaceName].ContainsKey(objectType.FullName))
                    {
                        cloneTypes[interfaceName].Add(objectType.FullName, ICloneType);
                    }
                    return ICloneType;
                }
                else
                    cloneTypes[interfaceName].Add(objectType.FullName, ICloneType);
            }
            else
            {
                cloneTypes.Add(interfaceName, new Dictionary<string, Type>());

                Type ICloneType = objectType.GetInterface(interfaceName, true);

                if (ICloneType != null)
                {
                    cloneTypes[interfaceName].Add(objectType.FullName, ICloneType);

                    return ICloneType;
                }
                else
                    cloneTypes[interfaceName].Add(objectType.FullName, ICloneType);
            }

            return null;
        }
    }
}
