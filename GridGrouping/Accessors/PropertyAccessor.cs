//
// Author: James Nies
// Date: 3/22/2005
// Description: The PropertyAccessor class provides fast dynamic access
//		to a property of a specified target class.
//
// *** This code was written by James Nies and has been provided to you, ***
// *** free of charge, for your use.  I assume no responsibility for any ***
// *** undesired events resulting from the use of this code or the		 ***
// *** information that has been provided with it .						 ***
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Collections;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Interfaces;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Accessors
{
	/// <summary>
	/// The PropertyAccessor class provides fast dynamic access
	/// to a property of a specified target class.
	/// </summary>
	public class PropertyAccessor : IPropertyAccessor
    {
        #region Fields & Proeprties

        internal bool isComplexPropertyAccessor = false;
        private bool isDerivedClassFound = false;
        internal bool callGetterFromBase = false;

        public bool IsDerivedClassFound
        {
            get { return isDerivedClassFound; }
            set
            {
                isDerivedClassFound = value;

                try
                {
                    if (mTargetType == null || !isDerivedClassFound)
                        return;

                    //
                    //Looking for base property getter and setter
                    //
                    if (!mCanRead)
                    {
                        if (mTargetType != null && mTargetType.BaseType != null)
                        {
                            PropertyInfo secInfo =
                                mTargetType.BaseType.GetProperty(mProperty, BindingFlags.Instance | BindingFlags.Public);

                            if (secInfo != null)
                            {
                                this.mCanRead = secInfo.CanRead;
                                this.mPropertyType = secInfo.PropertyType;
                                this.callGetterFromBase = true;
                            }
                        }
                    }

                    if (isDerivedClassFound)
                    {
                        if (this.mDerivedTargetType == null)
                        {
                            this.mDerivedTargetType = mTargetType;
                            //
                            //Change main type to base class
                            //
                            this.mTargetType = mTargetType.BaseType;

                            Init();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers.ExceptionsLogger.LogError(ex);
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
		/// Creates a new property accessor.
		/// </summary>
		/// <param name="targetType">Target object type.</param>
		/// <param name="property">Property name.</param>
        public PropertyAccessor(Type targetType, string property) :
            this(targetType, null, property)
        {

        }

        public PropertyAccessor(Type targetType, string property, bool isDerivedClassFound) :
            this(targetType, null, property, isDerivedClassFound)
        {

        }

        public PropertyAccessor(Type targetType, Type secTargetType, string property)
            : this(targetType, secTargetType, property, false)
        {

        }
        
		public PropertyAccessor(Type targetType, Type secTargetType, string property, bool isDerivedClassFound)
		{
            this.mTargetType = targetType;
            this.mProperty = property;
            this.mSecTargetType = secTargetType;
            this.isDerivedClassFound = isDerivedClassFound;

            if (property.Contains(".") && secTargetType != null)
            {
                isComplexPropertyAccessor = true;

                this.mProperty = property.Split('.')[0];
                this.mSecProperty = property.Split('.')[1];
            }

            PropertyInfo propertyInfo =
                targetType.GetProperty(mProperty, BindingFlags.Instance | BindingFlags.Public);
            
            //
            // Make sure the property exists
            //
            if (propertyInfo == null)
            {
                throw new PropertyAccessorException(
                    string.Format("Property \"{0}\" does not exist for type "
                    + "{1}.", mProperty, targetType));
            }
            else
            {
                this.mCanRead = propertyInfo.CanRead;
                this.mCanWrite = propertyInfo.CanWrite;
                this.mPropertyType = propertyInfo.PropertyType;

                try
                {
                    //
                    //Looking for base property getter and setter
                    //
                    if (!mCanRead)
                    {
                        if (mTargetType != null && mTargetType.BaseType != null)
                        {
                            PropertyInfo secInfo =
                                mTargetType.BaseType.GetProperty(mProperty, BindingFlags.Instance | BindingFlags.Public);

                            if (secInfo != null)
                            {
                                this.mCanRead = secInfo.CanRead;
                                this.mPropertyType = secInfo.PropertyType;
                                this.callGetterFromBase = true;
                            }
                        }
                    }

                    if (isDerivedClassFound)
                    {
                        this.mDerivedTargetType = mTargetType;
                        //
                        //Change main type to base class
                        //
                        this.mTargetType = mTargetType.BaseType;
                    }
                }
                catch (Exception ex)
                {
                    Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers.ExceptionsLogger.LogError(ex);
                }
            }
		}

        #endregion

        #region Main Getter / Setter

        /// <summary>
		/// Gets the property value from the specified target.
		/// </summary>
		/// <param name="target">Target object.</param>
		/// <returns>Property value.</returns>
		public object Get(object target)
		{
            if (mCanRead)
            {
                if (this.mEmittedPropertyAccessor == null)
                {
                    this.Init();
                }

                if (!isComplexPropertyAccessor)
                {
                    if (isDerivedClassFound)
                    {
                        if (target.GetType() == mDerivedTargetType)
                            return this.mDerivedEmittedPropertyAccessor.Get(target);
                        else
                            return this.mEmittedPropertyAccessor.Get(target);
                    }
                    else
                    {
                        return this.mEmittedPropertyAccessor.Get(target);
                    }
                }
                else
                {
                    object objTemp = this.mEmittedPropertyAccessor.Get(target);

                    if (objTemp != null && mSecondaryEmittedPropertyAccessor != null)
                        return this.mSecondaryEmittedPropertyAccessor.Get(objTemp);
                }

                return null;
            }
            else
            {
                throw new PropertyAccessorException(
                    string.Format("Property \"{0}\" does not have a get method.",
                    mProperty));
            }
		}

		/// <summary>
		/// Sets the property for the specified target.
		/// </summary>
		/// <param name="target">Target object.</param>
		/// <param name="value">Value to set.</param>
		public void Set(object target, object value)
		{
			if(mCanWrite)
			{
				if(this.mEmittedPropertyAccessor == null)
				{
					this.Init();
				}

				//
				// Set the property value
				//
				this.mEmittedPropertyAccessor.Set(target, value);
			}
			else
			{
				throw new PropertyAccessorException(
					string.Format("Property \"{0}\" does not have a set method.",
					mProperty));
			}
		}

        #endregion

        #region Properties & Fields

        /// <summary>
		/// Whether or not the Property supports read access.
		/// </summary>
		public bool CanRead
		{
			get
			{
				return this.mCanRead;
			}
		}

		/// <summary>
		/// Whether or not the Property supports write access.
		/// </summary>
		public bool CanWrite
		{
			get
			{
				return this.mCanWrite;
			}
		}

		/// <summary>
		/// The Type of object this property accessor was
		/// created for.
		/// </summary>
		public Type TargetType
		{
			get
			{
				return this.mTargetType;
			}
		}

        /// <summary>
        /// The Type of object this property accessor was
        /// created for.
        /// </summary>
        public Type SecondaryTargetType
        {
            get
            {
                return this.mSecTargetType;
            }
        }

		/// <summary>
		/// The Type of the Property being accessed.
		/// </summary>
		public Type PropertyType
		{
			get
			{
				return this.mPropertyType;
			}
		}

		private Type mTargetType;
        private string mProperty;

        public string Property
        {
            get { return mProperty; }
        }

        private Type mSecTargetType;
        private string mSecProperty;

        public string SecProperty
        {
            get { return mSecProperty; }
        }

        private Type mDerivedTargetType;
		private Type mPropertyType;
		private IPropertyAccessor mEmittedPropertyAccessor;
        private IPropertyAccessor mSecondaryEmittedPropertyAccessor;
        private IPropertyAccessor mDerivedEmittedPropertyAccessor;
		private Hashtable mTypeHash;
		private bool mCanRead;
		private bool mCanWrite;

        #endregion

        #region Initialization Methods

        /// <summary>
		/// This method generates creates a new assembly containing
		/// the Type that will provide dynamic access.
		/// </summary>
		public void Init()
		{
			this.InitTypes();

			// Create the assembly and an instance of the 
			// property accessor class.
			Assembly assembly = EmitAssembly();

			mEmittedPropertyAccessor = 
				assembly.CreateInstance("Property") as IPropertyAccessor;


            if (!string.IsNullOrEmpty(mSecProperty))
            {
                // Create the secondary assembly and an instance of the 
                // property accessor class.
                Assembly secondaryassembly = EmitSecondaryAssembly();

                mSecondaryEmittedPropertyAccessor =
                    secondaryassembly.CreateInstance("Property") as IPropertyAccessor;
            }

            if (isDerivedClassFound)
            { 
                // Create the derived class assembly and an instance of the 
                // property accessor class.
                Assembly derivedAssembly = EmitDerivedAssembly();

                mDerivedEmittedPropertyAccessor =
                    derivedAssembly.CreateInstance("Property") as IPropertyAccessor;
            }

			if(mEmittedPropertyAccessor == null)
			{
				throw new Exception("Unable to create property accessor.");
			}
		}

		/// <summary>
		/// Thanks to Ben Ratzlaff for this snippet of code
		/// http://www.codeproject.com/cs/miscctrl/CustomPropGrid.asp
		/// 
		/// "Initialize a private hashtable with type-opCode pairs 
		/// so i dont have to write a long if/else statement when outputting msil"
		/// </summary>
		private void InitTypes()
		{
			mTypeHash=new Hashtable();
			mTypeHash[typeof(sbyte)]=OpCodes.Ldind_I1;
			mTypeHash[typeof(byte)]=OpCodes.Ldind_U1;
			mTypeHash[typeof(char)]=OpCodes.Ldind_U2;
			mTypeHash[typeof(short)]=OpCodes.Ldind_I2;
			mTypeHash[typeof(ushort)]=OpCodes.Ldind_U2;
			mTypeHash[typeof(int)]=OpCodes.Ldind_I4;
			mTypeHash[typeof(uint)]=OpCodes.Ldind_U4;
			mTypeHash[typeof(long)]=OpCodes.Ldind_I8;
			mTypeHash[typeof(ulong)]=OpCodes.Ldind_I8;
			mTypeHash[typeof(bool)]=OpCodes.Ldind_I1;
			mTypeHash[typeof(double)]=OpCodes.Ldind_R8;
			mTypeHash[typeof(float)]=OpCodes.Ldind_R4;
		}

		/// <summary>
		/// Create an assembly that will provide the get and set methods.
		/// </summary>
		private Assembly EmitAssembly()
		{
            ////
            //// Create an assembly name
            ////
            //AssemblyName assemblyName = new AssemblyName();
            //assemblyName.Name = "PropertyAccessorAssembly";

            ////
            //// Create a new assembly with one module
            ////
            ////AssemblyBuilder newAssembly = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ////ModuleBuilder newModule = newAssembly.DefineDynamicModule("Module");

            ////		
            ////  Define a public class named "Property" in the assembly.
            ////			
            //TypeBuilder myType = 
            //	newModule.DefineType("Property", TypeAttributes.Public);

            ////
            //// Mark the class as implementing IPropertyAccessor. 
            ////
            //myType.AddInterfaceImplementation(typeof(IPropertyAccessor));

            //// Add a constructor
            //ConstructorBuilder constructor = 
            //	myType.DefineDefaultConstructor(MethodAttributes.Public);

            ////
            //// Define a method for the get operation. 
            ////
            //Type[] getParamTypes = new Type[] {typeof(object)};
            //Type getReturnType = typeof(object);
            //MethodBuilder getMethod = 
            //	myType.DefineMethod("Get", 
            //	MethodAttributes.Public | MethodAttributes.Virtual, 
            //	getReturnType, 
            //	getParamTypes);

            ////
            //// From the method, get an ILGenerator. This is used to
            //// emit the IL that we want.
            ////
            //ILGenerator getIL = getMethod.GetILGenerator();


            ////
            //// Emit the IL. 
            ////
            //MethodInfo targetGetMethod = this.mTargetType.GetMethod("get_" + this.mProperty);

            //if(targetGetMethod != null)
            //{

            //	getIL.DeclareLocal(typeof(object));
            //	getIL.Emit(OpCodes.Ldarg_1);								//Load the first argument 
            //																//(target object)

            //	getIL.Emit(OpCodes.Castclass, this.mTargetType);			//Cast to the source type

            //	getIL.EmitCall(OpCodes.Call, targetGetMethod, null);		//Get the property value

            //	if(targetGetMethod.ReturnType.IsValueType)
            //	{
            //		getIL.Emit(OpCodes.Box, targetGetMethod.ReturnType);	//Box if necessary
            //	}
            //	getIL.Emit(OpCodes.Stloc_0);								//Store it

            //	getIL.Emit(OpCodes.Ldloc_0);
            //}
            //else
            //{
            //	getIL.ThrowException(typeof(MissingMethodException));
            //}

            //getIL.Emit(OpCodes.Ret);


            ////
            //// Define a method for the set operation.
            ////
            //Type[] setParamTypes = new Type[] {typeof(object), typeof(object)};
            //Type setReturnType = null;
            //MethodBuilder setMethod = 
            //	myType.DefineMethod("Set", 
            //	MethodAttributes.Public | MethodAttributes.Virtual, 
            //	setReturnType, 
            //	setParamTypes);

            ////
            //// From the method, get an ILGenerator. This is used to
            //// emit the IL that we want.
            ////
            //ILGenerator setIL = setMethod.GetILGenerator();
            ////
            //// Emit the IL. 
            ////

            //MethodInfo targetSetMethod = this.mTargetType.GetMethod("set_" + this.mProperty, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            //if(targetSetMethod != null)
            //{
            //	Type paramType = targetSetMethod.GetParameters()[0].ParameterType;

            //	setIL.DeclareLocal(paramType);
            //	setIL.Emit(OpCodes.Ldarg_1);						//Load the first argument 
            //														//(target object)

            //	setIL.Emit(OpCodes.Castclass, this.mTargetType);	//Cast to the source type

            //	setIL.Emit(OpCodes.Ldarg_2);						//Load the second argument 
            //														//(value object)

            //	if(paramType.IsValueType)
            //	{
            //		setIL.Emit(OpCodes.Unbox, paramType);			//Unbox it 	
            //		if(mTypeHash[paramType]!=null)					//and load
            //		{
            //			OpCode load = (OpCode)mTypeHash[paramType];
            //			setIL.Emit(load);
            //		}
            //		else
            //		{
            //			setIL.Emit(OpCodes.Ldobj,paramType);
            //		}
            //	}
            //	else
            //	{
            //		setIL.Emit(OpCodes.Castclass, paramType);		//Cast class
            //	}

            //	setIL.EmitCall(OpCodes.Callvirt, 
            //		targetSetMethod, null);							//Set the property value
            //}
            //else
            //{
            //	setIL.ThrowException(typeof(MissingMethodException));
            //}

            //setIL.Emit(OpCodes.Ret);

            ////
            //// Load the type
            ////
            //myType.CreateType();
            AssemblyBuilder newAssembly = null;
			return newAssembly;
        }

        /// <summary>
        /// Create a secondary assembly that will provide the get and set methods.
        /// </summary>
        private Assembly EmitSecondaryAssembly()
        {
            //
            // Create an assembly name
            //
            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = "PropertyAccessorAssembly";

            //
            // Create a new assembly with one module
            //
            AssemblyBuilder newAssembly = null;
            //ModuleBuilder newModule = newAssembly.DefineDynamicModule("Module");

            ////		
            ////  Define a public class named "Property" in the assembly.
            ////			
            //TypeBuilder myType =
            //    newModule.DefineType("Property", TypeAttributes.Public);

            ////
            //// Mark the class as implementing IPropertyAccessor. 
            ////
            //myType.AddInterfaceImplementation(typeof(IPropertyAccessor));

            //// Add a constructor
            //ConstructorBuilder constructor =
            //    myType.DefineDefaultConstructor(MethodAttributes.Public);

            ////
            //// Define a method for the get operation. 
            ////
            //Type[] getParamTypes = new Type[] { typeof(object) };
            //Type getReturnType = typeof(object);
            //MethodBuilder getMethod =
            //    myType.DefineMethod("Get",
            //    MethodAttributes.Public | MethodAttributes.Virtual,
            //    getReturnType,
            //    getParamTypes);

            ////
            //// From the method, get an ILGenerator. This is used to
            //// emit the IL that we want.
            ////
            //ILGenerator getIL = getMethod.GetILGenerator();


            ////
            //// Emit the IL. 
            ////
            //MethodInfo targetGetMethod = this.mSecTargetType.GetMethod("get_" + this.mSecProperty);

            //if (targetGetMethod != null)
            //{

            //    getIL.DeclareLocal(typeof(object));
            //    getIL.Emit(OpCodes.Ldarg_1);								//Load the first argument 
            //    //(target object)

            //    getIL.Emit(OpCodes.Castclass, this.mSecTargetType);			//Cast to the source type

            //    getIL.EmitCall(OpCodes.Call, targetGetMethod, null);		//Get the property value

            //    if (targetGetMethod.ReturnType.IsValueType)
            //    {
            //        getIL.Emit(OpCodes.Box, targetGetMethod.ReturnType);	//Box if necessary
            //    }
            //    getIL.Emit(OpCodes.Stloc_0);								//Store it

            //    getIL.Emit(OpCodes.Ldloc_0);
            //}
            //else
            //{
            //    getIL.ThrowException(typeof(MissingMethodException));
            //}

            //getIL.Emit(OpCodes.Ret);


            ////
            //// Define a method for the set operation.
            ////
            //Type[] setParamTypes = new Type[] { typeof(object), typeof(object) };
            //Type setReturnType = null;
            //MethodBuilder setMethod =
            //    myType.DefineMethod("Set",
            //    MethodAttributes.Public | MethodAttributes.Virtual,
            //    setReturnType,
            //    setParamTypes);

            ////
            //// From the method, get an ILGenerator. This is used to
            //// emit the IL that we want.
            ////
            //ILGenerator setIL = setMethod.GetILGenerator();
            ////
            //// Emit the IL. 
            ////

            //MethodInfo targetSetMethod = this.mSecTargetType.GetMethod("set_" + this.mSecProperty);
            //if (targetSetMethod != null)
            //{
            //    Type paramType = targetSetMethod.GetParameters()[0].ParameterType;

            //    setIL.DeclareLocal(paramType);
            //    setIL.Emit(OpCodes.Ldarg_1);						//Load the first argument 
            //    //(target object)

            //    setIL.Emit(OpCodes.Castclass, this.mTargetType);	//Cast to the source type

            //    setIL.Emit(OpCodes.Ldarg_2);						//Load the second argument 
            //    //(value object)

            //    if (paramType.IsValueType)
            //    {
            //        setIL.Emit(OpCodes.Unbox, paramType);			//Unbox it 	
            //        if (mTypeHash[paramType] != null)					//and load
            //        {
            //            OpCode load = (OpCode)mTypeHash[paramType];
            //            setIL.Emit(load);
            //        }
            //        else
            //        {
            //            setIL.Emit(OpCodes.Ldobj, paramType);
            //        }
            //    }
            //    else
            //    {
            //        setIL.Emit(OpCodes.Castclass, paramType);		//Cast class
            //    }

            //    setIL.EmitCall(OpCodes.Callvirt,
            //        targetSetMethod, null);							//Set the property value
            //}
            //else
            //{
            //    setIL.ThrowException(typeof(MissingMethodException));
            //}

            //setIL.Emit(OpCodes.Ret);

            ////
            //// Load the type
            ////
            //myType.CreateType();

            return newAssembly;
        }

        /// <summary>
        /// Create a derived class assembly that will provide the get and set methods.
        /// </summary>
        private Assembly EmitDerivedAssembly()
        {
            //
            // Create an assembly name
            //
            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = "PropertyAccessorAssembly";

            //
            // Create a new assembly with one module
            //
            AssemblyBuilder newAssembly = null;
            //ModuleBuilder newModule = newAssembly.DefineDynamicModule("Module");

            ////		
            ////  Define a public class named "Property" in the assembly.
            ////			
            //TypeBuilder myType =
            //    newModule.DefineType("Property", TypeAttributes.Public);

            ////
            //// Mark the class as implementing IPropertyAccessor. 
            ////
            //myType.AddInterfaceImplementation(typeof(IPropertyAccessor));

            //// Add a constructor
            //ConstructorBuilder constructor =
            //    myType.DefineDefaultConstructor(MethodAttributes.Public);

            ////
            //// Define a method for the get operation. 
            ////
            //Type[] getParamTypes = new Type[] { typeof(object) };
            //Type getReturnType = typeof(object);
            //MethodBuilder getMethod =
            //    myType.DefineMethod("Get",
            //    MethodAttributes.Public | MethodAttributes.Virtual,
            //    getReturnType,
            //    getParamTypes);

            ////
            //// From the method, get an ILGenerator. This is used to
            //// emit the IL that we want.
            ////
            //ILGenerator getIL = getMethod.GetILGenerator();


            ////
            //// Emit the IL. 
            ////
            //MethodInfo targetGetMethod = null;
            
            //if(callGetterFromBase)
            //    targetGetMethod = this.TargetType.GetMethod("get_" + this.mProperty);
            //else
            //    targetGetMethod = this.mDerivedTargetType.GetMethod("get_" + this.mProperty);

            //if (targetGetMethod != null)
            //{

            //    getIL.DeclareLocal(typeof(object));
            //    getIL.Emit(OpCodes.Ldarg_1);								//Load the first argument 
            //    //(target object)

            //    getIL.Emit(OpCodes.Castclass, this.mDerivedTargetType);			//Cast to the source type

            //    getIL.EmitCall(OpCodes.Call, targetGetMethod, null);		//Get the property value

            //    if (targetGetMethod.ReturnType.IsValueType)
            //    {
            //        getIL.Emit(OpCodes.Box, targetGetMethod.ReturnType);	//Box if necessary
            //    }
            //    getIL.Emit(OpCodes.Stloc_0);								//Store it

            //    getIL.Emit(OpCodes.Ldloc_0);
            //}
            //else
            //{
            //    getIL.ThrowException(typeof(MissingMethodException));
            //}

            //getIL.Emit(OpCodes.Ret);


            ////
            //// Define a method for the set operation.
            ////
            //Type[] setParamTypes = new Type[] { typeof(object), typeof(object) };
            //Type setReturnType = null;
            //MethodBuilder setMethod =
            //    myType.DefineMethod("Set",
            //    MethodAttributes.Public | MethodAttributes.Virtual,
            //    setReturnType,
            //    setParamTypes);

            ////
            //// From the method, get an ILGenerator. This is used to
            //// emit the IL that we want.
            ////
            //ILGenerator setIL = setMethod.GetILGenerator();
            ////
            //// Emit the IL. 
            ////

            //MethodInfo targetSetMethod = this.mDerivedTargetType.GetMethod("set_" + this.mProperty);

            //if (targetSetMethod != null)
            //{
            //    Type paramType = targetSetMethod.GetParameters()[0].ParameterType;

            //    setIL.DeclareLocal(paramType);
            //    setIL.Emit(OpCodes.Ldarg_1);						//Load the first argument 
            //    //(target object)

            //    setIL.Emit(OpCodes.Castclass, this.mTargetType);	//Cast to the source type

            //    setIL.Emit(OpCodes.Ldarg_2);						//Load the second argument 
            //    //(value object)

            //    if (paramType.IsValueType)
            //    {
            //        setIL.Emit(OpCodes.Unbox, paramType);			//Unbox it 	
            //        if (mTypeHash[paramType] != null)					//and load
            //        {
            //            OpCode load = (OpCode)mTypeHash[paramType];
            //            setIL.Emit(load);
            //        }
            //        else
            //        {
            //            setIL.Emit(OpCodes.Ldobj, paramType);
            //        }
            //    }
            //    else
            //    {
            //        setIL.Emit(OpCodes.Castclass, paramType);		//Cast class
            //    }

            //    setIL.EmitCall(OpCodes.Callvirt,
            //        targetSetMethod, null);							//Set the property value
            //}
            //else
            //{
            //    setIL.ThrowException(typeof(MissingMethodException));
            //}

            //setIL.Emit(OpCodes.Ret);

            ////
            //// Load the type
            ////
            //myType.CreateType();

            return newAssembly;
        }

        #endregion
    }

    /// <summary>
    /// PropertyAccessorException class.
    /// </summary>
    public class PropertyAccessorException : Exception
    {
        public PropertyAccessorException(string message)
            : base(message)
        {
        }
    }
}