﻿/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 * 
 *  http://aws.amazon.com/apache2.0
 * 
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace Amazon.DynamoDBv2.DataModel
{
    /// <summary>
    /// Context object for using the DataModel mode of DynamoDB.
    /// Used to interact with the service, save/load objects, etc.
    /// </summary>
    public partial class DynamoDBContext : IDynamoDBContext
    {
        #region Save/serialize

        /// <summary>
        /// Saves an object to DynamoDB using passed-in configs.
        /// 
        /// Passed-in config overrides DynamoDBContextConfig on the context.
        /// Type must be marked up with DynamoDBTableAttribute and at least
        /// one public field/property with DynamoDBHashKeyAttribute.
        /// </summary>
        /// <typeparam name="T">Type to save as.</typeparam>
        /// <param name="value">Object to save.</param>
        /// <param name="operationConfig">Overriding configuration.</param>
        public void Save<T>(T value, DynamoDBOperationConfig operationConfig = null)
        {
            SaveHelper<T>(value, operationConfig, false);
        }

        #endregion

        #region Load/deserialize

        /// <summary>
        /// Loads an object from DynamoDB for the given hash-and-range primary key and using the given config.
        /// 
        /// Passed-in config overrides DynamoDBContextConfig on the context.
        /// Type must be marked up with DynamoDBTableAttribute and at least
        /// one public field/property with DynamoDBHashKeyAttribute.
        /// </summary>
        /// <typeparam name="T">Type to populate.</typeparam>
        /// <param name="hashKey">Hash key element of the target item.</param>
        /// <param name="rangeKey">Range key element of the target item.</param>
        /// <param name="operationConfig">Overriding configuration.</param>
        /// <returns>
        /// Object of type T, populated with properties of item loaded from DynamoDB.
        /// </returns>
        public T Load<T>(object hashKey, object rangeKey = null, DynamoDBOperationConfig operationConfig = null)
        {
            return LoadHelper<T>(hashKey, rangeKey, operationConfig, false);
        }

        /// <summary>
        /// Loads an object from DynamoDB for the given key and using the given config.
        /// The keyObject is a partially-specified instance, where the
        /// hash/range properties are equal to the key of the item you
        /// want to load.
        /// 
        /// Passed-in config overrides DynamoDBContextConfig on the context.
        /// Type must be marked up with DynamoDBTableAttribute and at least
        /// one public field/property with DynamoDBHashKeyAttribute.
        /// </summary>
        /// <typeparam name="T">Type to populate.</typeparam>
        /// <param name="keyObject">Key object defining the the target item.</param>
        /// <param name="operationConfig">Overriding configuration.</param>
        /// <returns>
        /// Object of type T, populated with properties of item loaded from DynamoDB.
        /// </returns>
        public T Load<T>(T keyObject, DynamoDBOperationConfig operationConfig = null)
        {
            return LoadHelper<T>(keyObject, operationConfig, false);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes an item in DynamoDB corresponding to given object.
        /// 
        /// Passed-in config overrides DynamoDBContextConfig on the context.
        /// If SkipVersionCheck=false, will check version of object before deleting.
        /// Type must be marked up with DynamoDBTableAttribute and at least
        /// one public field/property with DynamoDBHashKeyAttribute.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="value">Object to delete.</param>
        /// <param name="operationConfig">Overriding configuration.</param>
        public void Delete<T>(T value, DynamoDBOperationConfig operationConfig = null)
        {
            DeleteHelper<T>(value, operationConfig, false);
        }

        /// <summary>
        /// Deletes an item in DynamoDB corresponding to a given hash-and-range primary key.
        /// 
        /// No version check is done prior to delete.
        /// Type must be marked up with DynamoDBTableAttribute and at least
        /// one public field/property with DynamoDBHashKeyAttribute.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="hashKey">Hash key element of the object to delete.</param>
        /// <param name="rangeKey">Range key element of the object to delete.</param>
        /// <param name="operationConfig">Config object which can be used to override that table used.</param>
        public void Delete<T>(object hashKey, object rangeKey = null, DynamoDBOperationConfig operationConfig = null)
        {
            DeleteHelper<T>(hashKey, rangeKey, operationConfig, false);
        }

        #endregion

        #region BatchGet

        /// <summary>
        /// Creates a strongly-typed BatchGet object, allowing
        /// a batch-get operation against DynamoDB.
        /// </summary>
        /// <typeparam name="T">Type of objects to get</typeparam>
        /// <param name="operationConfig">Config object which can be used to override that table used.</param>
        /// <returns>Empty strongly-typed BatchGet object</returns>
        public BatchGet<T> CreateBatchGet<T>(DynamoDBOperationConfig operationConfig = null)
        {
            DynamoDBFlatConfig config = new DynamoDBFlatConfig(operationConfig, this.config);
            return new BatchGet<T>(this, config);
        }

        /// <summary>
        /// Creates a MultiTableBatchGet object, composed of multiple
        /// individual BatchGet objects.
        /// </summary>
        /// <param name="batches">Individual BatchGet objects</param>
        /// <returns>Composite MultiTableBatchGet object</returns>
        public MultiTableBatchGet CreateMultiTableBatchGet(params BatchGet[] batches)
        {
            return new MultiTableBatchGet(batches);
        }

        /// <summary>
        /// Issues a batch-get request with multiple batches.
        /// 
        /// Results are stored in the individual batches.
        /// </summary>
        /// <param name="batches">
        /// Configured BatchGet objects
        /// </param>
        public void ExecuteBatchGet(params BatchGet[] batches)
        {
            MultiTableBatchGet superBatch = new MultiTableBatchGet(batches);
            superBatch.Execute();
        }

        #endregion

        #region Batch Write

        /// <summary>
        /// Creates a strongly-typed BatchWrite object, allowing
        /// a batch-write operation against DynamoDB.
        /// </summary>
        /// <typeparam name="T">Type of objects to write</typeparam>
        /// <param name="operationConfig">Config object which can be used to override that table used.</param>
        /// <returns>Empty strongly-typed BatchWrite object</returns>
        public BatchWrite<T> CreateBatchWrite<T>(DynamoDBOperationConfig operationConfig = null)
        {
            DynamoDBFlatConfig config = new DynamoDBFlatConfig(operationConfig, this.config);
            return new BatchWrite<T>(this, config);
        }

        /// <summary>
        /// Creates a MultiTableBatchWrite object, composed of multiple
        /// individual BatchWrite objects.
        /// </summary>
        /// <param name="batches">Individual BatchWrite objects</param>
        /// <returns>Composite MultiTableBatchWrite object</returns>
        public MultiTableBatchWrite CreateMultiTableBatchWrite(params BatchWrite[] batches)
        {
            return new MultiTableBatchWrite(batches);
        }

        /// <summary>
        /// Issues a batch-write request with multiple batches.
        /// </summary>
        /// <param name="batches">
        /// Configured BatchWrite objects
        /// </param>
        public void ExecuteBatchWrite(params BatchWrite[] batches)
        {
            MultiTableBatchWrite superBatch = new MultiTableBatchWrite(batches);
            superBatch.Execute();
        }

        #endregion

        #region Scan

        /// <summary>
        /// Executes a Scan operation against DynamoDB, finding items
        /// that match the specified conditions.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="conditions">
        /// Conditions that the results should meet.
        /// </param>
        /// <returns>Lazy-loaded collection of results.</returns>
        public IEnumerable<T> Scan<T>(params ScanCondition[] conditions)
        {
            return Scan<T>(conditions, null);
        }

        /// <summary>
        /// Executes a Scan operation against DynamoDB, finding items
        /// that match the specified conditions.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="conditions">
        /// Conditions that the results should meet.
        /// </param>
        /// <param name="operationConfig">Config object which can be used to override that table used.</param>
        /// <returns>Lazy-loaded collection of results.</returns>
        public IEnumerable<T> Scan<T>(IEnumerable<ScanCondition> conditions, DynamoDBOperationConfig operationConfig)
        {
            Search scan = ConvertScan<T>(conditions, operationConfig);
            return FromSearch<T>(scan);
        }

        /// <summary>
        /// Executes a Scan operation against DynamoDB, finding items
        /// that match the specified conditions.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="scanConfig">Scan request object.</param>
        /// <param name="operationConfig">Config object which can be used to override the table used.</param>
        /// <returns>Lazy-loaded collection of results.</returns>
        public IEnumerable<T> FromScan<T>(ScanOperationConfig scanConfig, DynamoDBOperationConfig operationConfig = null)
        {
            if (scanConfig == null) throw new ArgumentNullException("scanConfig");

            Search search = ConvertFromScan<T>(scanConfig, operationConfig);
            return FromSearch<T>(search);
        }

        #endregion

        #region Query

        /// <summary>
        /// Executes a Query operation against DynamoDB, finding items
        /// that match the specified hash primary key.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="hashKeyValue">Hash key of the items to query.</param>
        /// <returns>Lazy-loaded collection of results.</returns>
        public IEnumerable<T> Query<T>(object hashKeyValue)
        {
            Search query = ConvertQueryByValue<T>(hashKeyValue, null, null);
            return FromSearch<T>(query);
        }

        /// <summary>
        /// Executes a Query operation against DynamoDB, finding items
        /// that match the specified hash primary key.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="hashKeyValue">Hash key of the items to query.</param>
        /// <param name="operationConfig">Config object which can be used to override the table used.</param>
        /// <returns>Lazy-loaded collection of results.</returns>
        public IEnumerable<T> Query<T>(object hashKeyValue, DynamoDBOperationConfig operationConfig)
        {
            Search query = ConvertQueryByValue<T>(hashKeyValue, null, operationConfig);
            return FromSearch<T>(query);
        }

        /// <summary>
        /// Executes a Query operation against DynamoDB, finding items
        /// that match the specified range element condition for a hash-and-range primary key.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="hashKeyValue">Hash key of the items to query.</param>
        /// <param name="op">Operation of the condition.</param>
        /// <param name="values">
        /// Value(s) of the condition.
        /// For all operations except QueryOperator.Between, values should be one value.
        /// For QueryOperator.Betwee, values should be two values.
        /// </param>
        /// <returns>Lazy-loaded collection of results.</returns>
        public IEnumerable<T> Query<T>(object hashKeyValue, QueryOperator op, params object[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentOutOfRangeException("values");

            return Query<T>(hashKeyValue, op, values, null);
        }

        /// <summary>
        /// Executes a Query operation against DynamoDB, finding items
        /// that match the specified range element condition for a hash-and-range primary key.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="hashKeyValue">Hash key of the items to query.</param>
        /// <param name="op">Operation of the condition.</param>
        /// <param name="values">
        /// Value(s) of the condition.
        /// For all operations except QueryOperator.Between, values should be one value.
        /// For QueryOperator.Betwee, values should be two values.
        /// </param>
        /// <param name="operationConfig">Config object which can be used to override the table used.</param>
        /// <returns>Lazy-loaded collection of results.</returns>
        public IEnumerable<T> Query<T>(object hashKeyValue, QueryOperator op, IEnumerable<object> values, DynamoDBOperationConfig operationConfig)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            Search query = ConvertQueryByValue<T>(hashKeyValue, op, values, operationConfig);
            return FromSearch<T>(query);
        }

        /// <summary>
        /// Executes a Query operation against DynamoDB, finding items
        /// that match the specified conditions.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="queryConfig">Query request object.</param>
        /// <returns>Lazy-loaded collection of results.</returns>
        public IEnumerable<T> FromQuery<T>(QueryOperationConfig queryConfig)
        {
            return FromQuery<T>(queryConfig, null);
        }

        /// <summary>
        /// Executes a Query operation against DynamoDB, finding items
        /// that match the specified conditions.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="queryConfig">Query request object.</param>
        /// <param name="operationConfig">Config object which can be used to override the table used.</param>
        /// <returns>Lazy-loaded collection of results.</returns>
        public IEnumerable<T> FromQuery<T>(QueryOperationConfig queryConfig, DynamoDBOperationConfig operationConfig)
        {
            if (queryConfig == null) throw new ArgumentNullException("queryConfig");

            Search search = ConvertFromQuery<T>(queryConfig, operationConfig);
            return FromSearch<T>(search);
        }

        #endregion

        #region Table methods

        /// <summary>
        /// Retrieves the target table for the specified type
        /// </summary>
        /// <typeparam name="T">Type to retrieve table for</typeparam>
        /// <returns>Table object</returns>
        public Table GetTargetTable<T>(DynamoDBOperationConfig operationConfig = null)
        {
            Type type = typeof(T);
            ItemStorageConfig storageConfig = ItemStorageConfigCache.GetConfig(type);
            Table table = GetTargetTable(storageConfig, new DynamoDBFlatConfig(operationConfig, this.config));
            Table copy = table.Copy(Table.DynamoDBConsumer.DocumentModel);
            return table;
        }

        #endregion

    }
}