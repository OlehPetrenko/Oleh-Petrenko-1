﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SortApp.BL.Properties;

namespace SortApp.BL.Repository
{
	/// <summary>
	/// Represents the implementation repository for Data.
	/// </summary>
	/// <owner>Oleh Petrenko</owner>
	public sealed class DataRepository : IRepository<Data>
	{
		/// <summary>
		/// Contains SqlDataAdapter object for using towards Data.
		/// </summary>
		/// <owner>Oleh Petrenko</owner>
		private SqlDataAdapter adapterData;

		/// <summary>
		/// Contains SqlDataAdapter object for using towards Iteration.
		/// </summary>
		/// <owner>Oleh Petrenko</owner>
		private readonly SqlDataAdapter adapterIteration;

		/// <summary>
		/// Contains SqlConnection object for using towards Iteration.
		/// </summary>
		/// <owner>Oleh Petrenko</owner>
		private readonly SqlConnection connection;

		/// <summary>
		/// Contains concrete implementation of dataAdapter factory.
		/// </summary>
		/// <owner>Oleh Petrenko</owner>
		private readonly DataAdapterFacroty dataAdapterFacroty;

		/// <summary>
		/// Contains DataSet object.
		/// </summary>
		/// <owner>Oleh Petrenko</owner>
		private DataSet dataSet;

		/// <summary>
		/// Creates new entity in the storage.
		/// </summary>
		/// <owner>Oleh Petrenko</owner>
		/// <param name="item">
		/// Entity for adding to storage.
		/// </param>
		public void Create(Data item)
		{
			this.adapterData = this.CreateDataAdapter(DataAdapterKindEnum.Data);
			this.dataSet = this.FillSchemaData();

			DataRow newRowData = this.dataSet.Tables["Data"].NewRow();
			newRowData["OriginalData"] = item.OriginalData;
			newRowData["SortedData"] = (object)item.SortedData ?? DBNull.Value;
			newRowData["Name"] = (object)item.Name ?? DBNull.Value;
			newRowData["KindOfSortingAlgorithm"] = item.KindOfSortingAlgorithm;
			this.dataSet.Tables["Data"].Rows.Add(newRowData);

			this.adapterData.Update(this.dataSet, "Data");

			int lastIndex = this.GetLastDataId();

			if (item.Iterations == null)
				return;

			foreach (Iteration iteration in item.Iterations)
			{
				DataRow newRowIteration = this.dataSet.Tables["Iteration"].NewRow();

				newRowIteration["ArrayState"] = iteration.ArrayState;
				newRowIteration["IndexOfFirstMovedElement"] = iteration.IndexOfFirstMovedElement;
				newRowIteration["IndexOfSecondMovedElement"] = iteration.IndexOfSecondMovedElement;
				newRowIteration["Number"] = iteration.Number;
				newRowIteration["DataId"] = lastIndex;

				this.dataSet.Tables["Iteration"].Rows.Add(newRowIteration);
			}

			this.adapterIteration.Update(dataSet, "Iteration");
		}

		/// <summary>
		/// Creates concrete implementation of dataAdapter.
		/// </summary>
		/// <owner>Oleh Petrenko</owner>
		/// <param name="kindOfDataAdapter">
		/// Kind of CreatorDataAdapter.
		/// </param>
		/// <returns>
		/// Concrete implementation of dataAdapter.
		/// </returns>
		private SqlDataAdapter CreateDataAdapter(DataAdapterKindEnum kindOfDataAdapter)
		{
			return this.dataAdapterFacroty.CreateAdapter(kindOfDataAdapter).CreateAdapter(this.connection);
		}

		/// <summary>
		/// Creates Data entity from DataRow entity.
		/// </summary>
		/// <owner>Oleh Petrenko</owner>
		/// <param name="row">
		/// Original row as DataRow.
		/// </param>
		/// <param name="iterationRows">
		/// Iteration rows.
		/// </param>
		/// <returns>
		/// Data object created from DataRow.
		/// </returns>
		private Data CreateDataFromDataRow(DataRow row, DataRowCollection iterationRows)
		{
			Data data = new Data
			{
				Id = (int)row["Id"],
				OriginalData = (string)row["OriginalData"],
				SortedData = row["SortedData"] != DBNull.Value ? (string)row["SortedData"] : null,
				Name = row["Name"] != DBNull.Value ? (string)row["Name"] : null,
				KindOfSortingAlgorithm = (SortingAlgorithmKind)Enum.Parse(typeof(SortingAlgorithmKind), ((int)row["KindOfSortingAlgorithm"]).ToString()),
				Iterations = new List<Iteration>(iterationRows.Count)
			};

			foreach (DataRow iterationRow in iterationRows)
			{
				data.Iterations.Add(new Iteration(
					(int)iterationRow["IndexOfFirstMovedElement"],
					(int)iterationRow["IndexOfSecondMovedElement"],
					(int)iterationRow["Number"])
				{
					ArrayState = (string)iterationRow["ArrayState"]
				});
			}

			return data;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DataRepository"/> class.
		/// </summary>
		/// <owner>Oleh Petrenko</owner>
		public DataRepository()
		{
			this.connection = new SqlConnection(Settings.Default.DbConnect);
			this.dataAdapterFacroty = new DataAdapterFacroty();

			this.adapterData = this.CreateDataAdapter(DataAdapterKindEnum.Data);
			this.adapterIteration = this.CreateDataAdapter(DataAdapterKindEnum.Iteration);

			this.dataSet = this.FillSchemaData();
		}

		/// <summary>
		/// Removes entity of the storage.
		/// </summary>
		/// <owner>Oleh Petrenko</owner>
		/// <param name="item">
		/// Entity for removing.
		/// </param>
		public void Delete(Data item)
		{
			this.adapterData = this.CreateDataAdapter(DataAdapterKindEnum.Data);
			this.dataSet = this.FillSchemaData();

			this.adapterData.SelectCommand.Parameters["@Id"].Value = item.Id;

			this.adapterData.Fill(this.dataSet, "Data");

			this.dataSet.Tables["Data"].Rows.Find(item.Id).Delete();

			this.adapterData.Update(dataSet, "Data");
		}

		/// <summary>
		/// Returns DataSet object with filled schema.
		/// </summary>
		/// <owner>Oleh Petrenko</owner>
		/// <returns>
		/// The DataSet object with filled schema.
		/// </returns>
		private DataSet FillSchemaData()
		{
			DataSet dataSet = new DataSet();

			this.adapterData.FillSchema(dataSet, SchemaType.Mapped, "Data");
			this.adapterIteration.FillSchema(dataSet, SchemaType.Mapped, "Iteration");

			return dataSet;
		}

		/// <summary>
		/// Returns all elements from storage.
		/// </summary>
		/// <owner>Oleh Petrenko</owner>
		/// <returns>
		/// List which contains all elements from storage.
		/// </returns>
		public List<Data> GellAll()
		{
			this.adapterData = this.CreateDataAdapter(DataAdapterKindEnum.Data);
			this.dataSet = this.FillSchemaData();

			List<Data> arrays = new List<Data>();

			this.adapterData.SelectCommand = new SqlCommand("SELECT * FROM Data", this.connection);
			this.adapterData.Fill(this.dataSet, "Data");

			foreach (DataRow row in this.dataSet.Tables["Data"].Rows)
			{
				this.dataSet.Tables["Iteration"].Clear();

				this.adapterIteration.SelectCommand.Parameters["@DataId"].Value = row["Id"];
				this.adapterIteration.Fill(this.dataSet, "Iteration");

				DataRowCollection iterationRows = this.dataSet.Tables["Iteration"].Rows;
				arrays.Add(CreateDataFromDataRow(row, iterationRows));
			}

			return arrays;
		}

		/// <summary>
		/// Returns entity from storage by Id.
		/// </summary>
		/// <owner>Oleh Petrenko</owner>
		/// <param name="id">
		/// Entity identity in storage.
		/// </param>
		/// <returns>
		/// Entity from storage.
		/// </returns>
		public Data GetById(int id)
		{
			this.adapterData = this.CreateDataAdapter(DataAdapterKindEnum.Data);
			this.dataSet = this.FillSchemaData();

			this.adapterData.SelectCommand.Parameters["@Id"].Value = id;
			this.adapterData.Fill(this.dataSet, "Data");

			this.adapterIteration.SelectCommand.Parameters["@DataId"].Value = id;
			this.adapterIteration.Fill(this.dataSet, "Iteration");

			DataRow dataRow = this.dataSet.Tables["Data"].Rows[0];
			DataRowCollection iterationRows = this.dataSet.Tables["Iteration"].Rows;

			return CreateDataFromDataRow(dataRow, iterationRows);
		}

		/// <summary>
		/// Returns max id from Data.
		/// </summary>
		/// <owner>Oleh Petrenko</owner>
		/// <returns>
		/// Max id.
		/// </returns>
		private int GetLastDataId()
		{
			int lastId = 0;

			using (SqlConnection connection = new SqlConnection(Settings.Default.DbConnect))
			{
				SqlCommand cmd = new SqlCommand("SELECT MAX(Id) FROM Data", connection);

				connection.Open();

				SqlDataReader dr = cmd.ExecuteReader();
				if (dr.Read())
				{
					lastId = dr.GetInt32(0);
				}

				connection.Close();
			}

			return lastId;
		}

		/// <summary>
		/// Updates entity in the storage.
		/// </summary>
		/// <owner>Oleh Petrenko</owner>
		/// <param name="item">
		/// Entity for updating.
		/// </param>
		public void Update(Data item)
		{
			this.adapterData = this.CreateDataAdapter(DataAdapterKindEnum.Data);
			this.dataSet = this.FillSchemaData();

			this.adapterData.SelectCommand.Parameters["@Id"].Value = item.Id;

			this.adapterData.Fill(this.dataSet, "Data");

			DataRow newRow = this.dataSet.Tables["Data"].Rows.Find(item.Id);
			newRow["OriginalData"] = item.OriginalData;
			newRow["SortedData"] = (object)item.SortedData ?? DBNull.Value;
			newRow["Name"] = (object)item.Name ?? DBNull.Value;

			this.adapterData.Update(this.dataSet, "Data");
		}
	}
}
