﻿using API;
using FluentValidation;
using PDFapi.Resources;
using System;

namespace PDFapi.Template
{
    /// <summary>
    /// Base Abstract class to allow for the template method pattern of Create, Read, Update and Delete objects from our model
    /// 
    /// </summary>
    internal abstract class BaseTemplate_Read<T, V> : BaseTemplate<T, V>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="request"></param>
        /// <param name="validator"></param>
        protected BaseTemplate_Read(JSONRPC_API request, IValidator<T> validator) : base(request, validator)
        {
        }

        /// <summary>
        /// Success
        /// </summary>
        protected override void OnExecutionSuccess()
        {
            Log.Instance.Debug("Record Read");
            //If a cache DTO exists, then we have been given a directive to store the results in the cache
            if (cDTO != null)
                MemCacheD.Store_BSO<dynamic>(cDTO.Namespace, cDTO.ApiName, cDTO.Method, DTO, Response.data, cDTO.TimeLimit, cDTO.Cas + cDTO.Domain);

        }

        /// <summary>
        /// Error
        /// </summary>
        protected override void OnExecutionError()
        {
            Log.Instance.Debug("No record read");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <returns></returns>
        public BaseTemplate_Read<T, V> Read()
        {
            try
            {
                //Run the parameters through the cleanse process
                dynamic cleansedParams = Cleanser.Cleanse(Request.parameters);


                try
                {
                    DTO = GetDTO(cleansedParams);
                }
                catch
                {
                    throw new InputFormatException();
                }

                DTO = Sanitizer.Sanitize(DTO);

                DTOValidationResult = Validator.Validate(DTO);

                if (!DTOValidationResult.IsValid)
                {
                    OnDTOValidationError();

                    return this;
                }

                //See if there's a cache in the process
                if (MethodReader.MethodHasAttribute(Request.method, "CacheRead"))
                {
                    cDTO = new CacheMetadata("CacheRead", Request.method, DTO);
                    MemCachedD_Value cache = MemCacheD.Get_BSO<dynamic>(cDTO.Namespace, cDTO.ApiName, cDTO.Method, DTO);
                    if (cache.hasData)
                    {
                        Response.data = cache.data;
                        return this;
                    }
                }

                // The Actual Read should happen here by the specific class!
                if (!Execute())
                {
                    OnExecutionError();
                    return this;
                }

                OnExecutionSuccess();

                return this;
            }
            catch (UnmatchedParametersException unmatchException)
            {
                Log.Instance.Debug(unmatchException);
                OnDTOValidationError();
                return this;
            }
            catch (FormatException formatException)
            {
                //A FormatException error has been caught, log the error and return a message to the caller
                Log.Instance.Error(formatException);
                Response.error = Label.Get("error.schema");
                return this;
            }
            catch (Exception ex)
            {
                //An error has been caught,  log the error and return a message to the caller
                Log.Instance.Error(ex);
                Response.error = Label.Get("error.exception");

                return this;
            }
            finally
            {
                Dispose();
            }
        }
    }
}
