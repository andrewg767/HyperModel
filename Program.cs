using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperModel
{
    class Program
    {
        static void Main(string[] args)
        {
            // import relevant classes
            NeuralNetworks.TDSimNetwork NN = new NeuralNetworks.TDSimNetwork();
            NeuralNetworks.NeuroEvo NE = new NeuralNetworks.NeuroEvo();
            DataIO.DataExport DE = new DataIO.DataExport();

            // data management stuff
            NN.NumLinesToRemove = 2;
            NN.NumPoints = 25;
            NN.NumControlVars = 2;
            NN.NumValidationPoints = 100;
            
            // set network learning variables for learning model
            NN.NumInputs = 21;
            NN.NumHidden = 15;
            NN.NumOutputs = 19;
            NN.WeightInitSTD = 0.75;
            NN.Eta = 0.1; //.25 // was most recently 0.2
            NN.Episodes = 1500;
            NN.Momentum = 0.7; //.5 // was most recently 0.7
            NN.NumRestarts = 750;
            NN.Shuffle = NeuralNetworks.ToggleShuffle.yes;
            NN.NumMSEReportingPoints = NN.Episodes;
            NN.DataExtension = "ColdAir";

            // train network 
            //NN.TrainNetwork();

            // import network and test it
            NN.ImportTrainedNetwork();
            //NN.TDSimNoControl(11);

            // set simulation parameters: initial state, time steps, and desired setpoint
            NN.InitialState = NN.Outputs[0];
            NN.SimTimeSteps = 200;
            NN.SetPoint.Add(NN.Outputs[200]);
            NN.SetPoint.Add(NN.Outputs[100]);


            // set neuroevolutionary parameters
            NE.NumInputs = NN.NumOutputs;
            NE.NumHidden = 10;
            NE.NumOutputs = 2;
            NE.WeightInitSTD = 2.0;
            NE.PopSize = 20;
            NE.MutateSTD = 1.0;
            NE.NumMutationsMatrix1 = 25;
            NE.NumMutationsMatrix2 = 3;
            NE.Domain = NN;
            NE.Epochs = 25000;
            NE.StatRuns = 5;
            NE.FitType = NeuralNetworks.FitnessType.fStatic;

            // run neuroevolutionary control algorithms, and export data
            double[,] eaData = NE.StatRunsEA();
            DE.Export2DArray(eaData, "eaData");

            // pick a parameter to plot controlled var vs. desired setpoint
            int parameterOfInterest = 11;

            // simulate using learned controller, and export true vs. desired datapoints
            List<double[]> allStateData = NN.TDSim(NE.BestNetwork);
            double[,] neControlData = new double[NN.SimTimeSteps, 2];
            double[,] setPointData = new double[NN.SimTimeSteps, 2];
            for (int i = 0; i < NN.SimTimeSteps; i++)
            {
                neControlData[i, 0] = Convert.ToDouble(i);
                neControlData[i, 1] = allStateData[i][parameterOfInterest];
                setPointData[i, 0] = Convert.ToDouble(i);
                setPointData[i, 1] = NN.SetPoint[0][parameterOfInterest];
            }

            DE.Export2DArray(neControlData, "neControlData");
            DE.Export2DArray(setPointData, "setPointData");

            
            Console.WriteLine("Press ENTER to Continue...");
            Console.ReadLine();
        }
    }
}
