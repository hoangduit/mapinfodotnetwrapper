﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using MapInfo.Wrapper.DataAccess.LINQ.SQL;
using MapInfo.Wrapper.DataAccess.Row;
using MapInfo.Wrapper.DataAccess.Row.Enumerators;
using MapInfo.Wrapper.Mapinfo;


namespace MapInfo.Wrapper.DataAccess.LINQ
{
    public class MapinfoProvider : IQueryProvider
    {
        private readonly MapInfoSession misession;
        private readonly MaterializerFactory entityfactory;
        private readonly DataReaderFactory readerfactory;

        public MapinfoProvider(MapInfoSession miSession, MaterializerFactory factory)
        {
            this.misession = miSession;
            this.entityfactory = factory;
            this.readerfactory = new DataReaderFactory(miSession);
        }

        public object Execute(Expression expression)
        {
            TranslateResult result = this.Translate(expression);

            if (result.SQLCommand != null)
            {
                this.misession.Do(result.SQLCommand);
            }

            Type elementType = TypeSystem.GetElementType(expression.Type);


            IMapInfoDataReader reader = this.readerfactory.GetReaderFor(result.TableName);
            ITable table = this.misession.Tables.GetTable(result.TableName);

            EntityMaterializer materializer = this.entityfactory.CreateMaterializerFor(table, reader);

            if (result.Projector != null)
            {
                Delegate projector = result.Projector.Compile();

                return Activator.CreateInstance(typeof(ProjectionReader<>).MakeGenericType(elementType),
                                                new object[] { reader, projector });
            }

            return Activator.CreateInstance(typeof(RowList<>).MakeGenericType(elementType),
                                            new object[] { reader, materializer });
        }

        public string GetQueryString(Expression expression)
        {
            return this.Translate(expression).SQLCommand;
        }

        public TranslateResult Translate(Expression expression)
        {
            expression = Evaluator.PartialEval(expression);
            return new QueryTranslator( ).Translate(expression);
        }

        #region IQueryProvider Members

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            return (IQueryable<TElement>)Activator.CreateInstance(typeof(LinqQuery<>).MakeGenericType(elementType),
                                                                  new object[] { this, expression });
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return this.CreateQuery(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)this.Execute(expression);
        }

        #endregion
    }

    [DebuggerNonUserCode]
    [DebuggerStepThrough]
    internal static class TypeSystem
    {

        internal static Type GetElementType(Type seqType)
        {
            Type ienum = FindIEnumerable(seqType);
            if (ienum == null)
                return seqType;
            return ienum.GetGenericArguments( )[0];
        }

        private static Type FindIEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
                return null;
            if (seqType.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType( ));
            if (seqType.IsGenericType)
            {
                foreach (Type arg in seqType.GetGenericArguments( ))
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType))
                    {
                        return ienum;
                    }
                }
            }

            Type[] ifaces = seqType.GetInterfaces( );

            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null)
                        return ienum;
                }
            }
            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.BaseType);
            }
            return null;
        }
    }
}
