﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ANN;
using ANN.Models;
using Npgsql;
using NpgsqlTypes;

namespace ANN.Models
{
    public class Layer : ILayer
    {

        private static ANNDBEntities1 db = new ANNDBEntities1();

        private int number;

        private List<Neuron> neuronsOfLayer;

        public double[] Output { get; private set; }

        public Layer(int n)
        {
            neuronsOfLayer = new List<Neuron>();
            number = n;
            var list = db.neuron.Where(a => a.layer == number).ToList(); 
            if(list.Count() < 125)
            {
                FillNeuron.Filling();
            }
            list = db.neuron.Where(a => a.layer == number).ToList();
            foreach (var neuron in list)
            {
                var weights = db.Database.SqlQuery<double>("SELECT unnest(weight) FROM neuron WHERE neuron.Neuron_id = @id", new NpgsqlParameter("@id", neuron.neuron_id)).ToArray();
                neuronsOfLayer.Add(new Neuron(weights));
            }
        }

        public void Calculate(double[] input)
        {
            for (int i = 0; i < 125; i++)
            {
                Output[i] = neuronsOfLayer[i].Calculate(input);
            }
        }

        public void Learning(double misstake, double coof)
        {
            foreach (var neuron in neuronsOfLayer)
            {
                neuron.Learning(misstake, coof);
            }
        }

        public void SaveWeights()
        {
                for (int i = 0; i < 125; i++)
                {
                    db.Database.ExecuteSqlCommand("UPDATE neuron SET weight = @weight, layer = @layer WHERE neuron_id = @id",
                                                    new object[] {
                                                    new NpgsqlParameter("id",(number - 1) * 125 + i),
                                                    new NpgsqlParameter {ParameterName = "weihgt",
                                                                        NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Double,
                                                                        Value = neuronsOfLayer[i].weights },
                                                    new NpgsqlParameter("layer", number)});
                }
        }
    }
}