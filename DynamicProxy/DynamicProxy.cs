using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

namespace Gogo.DynamicProxy
{
    public class DynamicProxy
    {
        public static TInterface Create<TInterface, TImpl>(TImpl imp)
            where TInterface : class
            where TImpl : class
        {
            Type type = DynimicTypeGenerater<TInterface, TImpl>();
            return Activator.CreateInstance(type, imp) as TInterface;
        }
        private static Type DynimicTypeGenerater<TInterface, TImpl>()
            where TInterface : class
            where TImpl : class
        {
            Type typeInterface = typeof(TInterface);
            Type typeImpl = typeof(TImpl);

            // properties
            PropertyInfo[] interfacePropertys = typeInterface.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // methods
            MethodInfo[] interfaceMethods = typeInterface.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            List<MethodInfo> methodList = new List<System.Reflection.MethodInfo>();
            foreach (var item in interfaceMethods)
            {
                if (!item.IsSpecialName)
                    methodList.Add(item);
            }

            MethodInfo[] implMethods = typeImpl.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            AssemblyName abName = new AssemblyName("DynamicTypes");
            AssemblyBuilder abBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(abName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder mBuilder = abBuilder.DefineDynamicModule(abName.Name, abName.Name + ".dll");

            TypeBuilder tBuilder = mBuilder.DefineType(GetDynamicTypeName<TInterface, TImpl>(), TypeAttributes.Public, null, new Type[] { typeInterface });

            FieldBuilder fBuilder = tBuilder.DefineField("__wrappedInstance", typeImpl, FieldAttributes.Private);

            ConstructorBuilder ctorBuilder = tBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new Type[] { typeImpl });

            ILGenerator ctorBuilderIL = ctorBuilder.GetILGenerator();
            ctorBuilderIL.Emit(OpCodes.Ldarg_0);
            ctorBuilderIL.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
            ctorBuilderIL.Emit(OpCodes.Ldarg_0);
            ctorBuilderIL.Emit(OpCodes.Ldarg_1);
            ctorBuilderIL.Emit(OpCodes.Stfld, fBuilder);
            ctorBuilderIL.Emit(OpCodes.Ret);

            foreach (var item in interfacePropertys)
            {
                MethodInfo getMi = FindGetMethodInfo(implMethods, item);
                MethodInfo setMi = FindSetMethodInfo(implMethods, item);
                CreateProperty(tBuilder, fBuilder, item, getMi, setMi);
            }

            foreach (var item in interfaceMethods)
            {
                MethodInfo instanceMi = FindMethodInfo(implMethods, item);
                CreateMethod(tBuilder, fBuilder, item, instanceMi);
            }

            return tBuilder.CreateType();
        }

        private static Type DynamicTypeGenerater<TInterface,TImpl>(IInterceptor intercepter)
        {
            Type interfaceType = typeof(TInterface);
            Type implType = typeof(TImpl);

            MethodInfo[] interfaceMethods = interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] interfaceProperties = interfaceType.GetProperties(BindingFlags.Instance|BindingFlags.Public);

            MethodInfo[] ImplMethods = implType.GetMethods(BindingFlags.Public|BindingFlags.Instance);

            AssemblyName abName = new System.Reflection.AssemblyName("_AssemblyName");
            AssemblyBuilder abBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(abName, AssemblyBuilderAccess.RunAndSave);

            ModuleBuilder mdBuilder = abBuilder.DefineDynamicModule(abName.Name, abName.Name + ".dll");

            TypeBuilder typeBuilder = mdBuilder.DefineType(
                "_dynamictype",
                TypeAttributes.Class | TypeAttributes.Public,
                null,
                new Type[]{interfaceType});
            FieldBuilder fdBuilder = typeBuilder.DefineField("_wappedImpl", implType, FieldAttributes.Private);
            BuildProxyConstructor(implType, typeBuilder,fdBuilder);

            foreach(var property in interfaceProperties)
            {
                MethodInfo setM = GetSetPropertyMethod(property, ImplMethods);
                MethodInfo getM = GetGetPropertyMethod(property, ImplMethods);
                GenerateProperty(typeBuilder, fdBuilder, property, setM, getM);
            }

            foreach(var method in interfaceMethods)
            {

            }
        }
        private static void GenerateProperty(TypeBuilder typeBuilder,FieldBuilder fdBuilder,PropertyInfo property,MethodInfo setM,MethodInfo getM)
        {
            string propertyName = property.Name;
            Type propertyType = property.PropertyType;
            PropertyBuilder pBuilder = typeBuilder.DefineProperty(propertyName,
                PropertyAttributes.HasDefault,
                propertyType,
                null);
            MethodAttributes mAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Virtual|MethodAttributes.Final;
            MethodBuilder setBuilder = typeBuilder.DefineMethod("set_" + propertyName, mAttributes, propertyType,Type.EmptyTypes);
            var setIL = setBuilder.GetILGenerator();
            if (setM == null)
            {
                setIL.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(Type.EmptyTypes));
                setIL.Emit(OpCodes.Throw);
            }
            else
            {
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldfld, fdBuilder);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Callvirt, setM);
                setIL.Emit(OpCodes.Ret);
            }
            
            MethodBuilder getBuilder = typeBuilder.DefineMethod("get_"+propertyName,mAttributes,propertyType,Type.EmptyTypes);
            var getIL = getBuilder.GetILGenerator();
            if(getM==null)
            {
                getIL.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(Type.EmptyTypes));
                getIL.Emit(OpCodes.Throw);
            }
            else
            {
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, fdBuilder);
                getIL.Emit(OpCodes.Ldarg_1);
                getIL.Emit(OpCodes.Callvirt, getM);
                getIL.Emit(OpCodes.Ret);
            }
            pBuilder.SetGetMethod(getBuilder);
            pBuilder.SetSetMethod(setBuilder);
        }


        private static MethodInfo GetSetPropertyMethod(PropertyInfo pInfo,MethodInfo[] methods)
        {
            if(methods!=null)
            {
                foreach(var method in methods)
                {
                    if(method.Name=="set_"+pInfo.Name)
                    {
                        return method;
                    }
                }
            }
            return null;
        }
        private static MethodInfo GetGetPropertyMethod(PropertyInfo pInfo,MethodInfo[] methods)
        {
            if (methods != null)
            {
                foreach (var method in methods)
                {
                    if (method.Name == "get_" + pInfo.Name)
                    {
                        return method;
                    }
                }
            }
            return null;
        }

        private static void BuildProxyConstructor(Type implType, TypeBuilder typeBuilder, FieldBuilder fdBuilder)
        {
            ConstructorBuilder ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { implType });
            var ctorIL = ctorBuilder.GetILGenerator();
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Ldarg_1);
            ctorIL.Emit(OpCodes.Stfld, fdBuilder);
            ctorIL.Emit(OpCodes.Ret);
        }
        private static void CreateMethod(TypeBuilder tb, FieldBuilder fbInstance, MethodInfo mi, MethodInfo instanceMi)
        {
            List<Type> paramTyleList = new List<Type>();
            foreach (var item in mi.GetParameters())
                paramTyleList.Add(item.ParameterType);

            MethodBuilder mb = tb.DefineMethod(
              mi.Name,
              MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
              mi.ReturnType,
              paramTyleList.ToArray());

            ILGenerator il = mb.GetILGenerator();
            if (instanceMi == null)
            {
                il.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(new Type[] { }));
                il.Emit(OpCodes.Throw);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fbInstance);
                switch (paramTyleList.Count)
                {
                    case 0:
                        break;
                    case 1:
                        il.Emit(OpCodes.Ldarg_1);
                        break;
                    case 2:
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldarg_2);
                        break;
                    case 3:
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldarg_2);
                        il.Emit(OpCodes.Ldarg_3);
                        break;
                    default:
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldarg_2);
                        il.Emit(OpCodes.Ldarg_3);

                        Int32 sCount = Math.Min(paramTyleList.Count, 127);
                        for (int i = 4; i <= sCount; i++)
                        {
                            il.Emit(OpCodes.Ldarg_S, i);
                        }

                        for (int i = 128; i <= paramTyleList.Count; i++)
                        {
                            il.Emit(OpCodes.Ldarg, i);
                        }

                        break;
                }

                il.Emit(OpCodes.Callvirt, instanceMi);
                il.Emit(OpCodes.Ret);
                
            }
        }

        private static MethodInfo FindMethodInfo(MethodInfo[] methodsImpl, MethodInfo mi)
        {
            foreach (var item in methodsImpl)
            {
                if (item.Name.Equals(mi.Name)
                    && !item.IsSpecialName)
                    return item;
            }
            return null;
        }

        private static void CreateProperty(TypeBuilder tb, FieldBuilder fbInstance, PropertyInfo pi, MethodInfo getMi, MethodInfo setMi)
        {
            String name = pi.Name;
            Type type = pi.PropertyType;

            PropertyBuilder pb = tb.DefineProperty(
                name,
                PropertyAttributes.HasDefault,
                type,
                null);

            MethodAttributes getSetAttr = MethodAttributes.Public |
                MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
            MethodBuilder mbGetAccessor = tb.DefineMethod(
                "get_" + name,
                getSetAttr,
                type,
                Type.EmptyTypes);

            ILGenerator getIL = mbGetAccessor.GetILGenerator();
            if (getMi == null)
            {
                getIL.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(new Type[] { }));
                getIL.Emit(OpCodes.Throw);
            }
            else
            {
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, fbInstance);
                getIL.Emit(OpCodes.Callvirt, getMi);
                getIL.Emit(OpCodes.Ret);
            }

            MethodBuilder mbSetAccessor = tb.DefineMethod(
                "set_" + name,
                getSetAttr,
                null,
                new Type[] { type });

            ILGenerator setIL = mbSetAccessor.GetILGenerator();
            if (setMi == null)
            {
                setIL.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(new Type[] { }));
                setIL.Emit(OpCodes.Throw);
            }
            else
            {
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldfld, fbInstance);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Callvirt, setMi);
                setIL.Emit(OpCodes.Ret);
            }

            pb.SetGetMethod(mbGetAccessor);
            pb.SetSetMethod(mbSetAccessor);
        }

        private static MethodInfo FindGetMethodInfo(MethodInfo[] implMethods, PropertyInfo property)
        {
            foreach (var item in implMethods)
            {
                if (item.Name.Equals("get_" + property.Name)
                    && item.IsSpecialName)
                    return item;
            }

            return null;
        }

        private static MethodInfo FindSetMethodInfo(MethodInfo[] implMethods, PropertyInfo property)
        {
            foreach (var item in implMethods)
            {
                if (item.Name.Equals("set_" + property.Name)
                    && item.IsSpecialName)
                    return item;
            }

            return null;
        }

        private static string GetDynamicTypeName<TInterface, TImpl>()
            where TImpl : class
            where TInterface : class
        {
            return "_DynamicType" + typeof(TInterface).ToString() + "_" + typeof(TImpl).ToString();
        }
    }
}
