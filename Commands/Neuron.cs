using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Curious_Cat
{
    public enum NeuronType
    {
        Input = 1,
        Hidden = 2,
        Output = 3
    }

    [Serializable]
    public class NeuralNetwork
    {
        public List<List<Neuron>> layers;

        public enum WeightType
        {
            Random = 1,
            None = 2
        }

        public NeuralNetwork()
        {

        }

        //конструктор нейросети
        public NeuralNetwork(WeightType type, params int[] counts)
        {
            layers = new List<List<Neuron>>();

            int i = 0;

            layers.Add(new List<Neuron>());
            for (int j = 0; j < counts[i]; j++)
            {
                layers[i].Add(new Neuron(NeuronType.Input));
            }
            i++;

            while (i < counts.Length - 1)
            {
                layers.Add(new List<Neuron>());
                for (int j = 0; j < counts[i]; j++)
                {
                    layers[i].Add(new Neuron(NeuronType.Hidden));
                }
                i++;
            }

            layers.Add(new List<Neuron>());
            for (int j = 0; j < counts[i]; j++)
            {
                layers[i].Add(new Neuron(NeuronType.Output));
            }

            if (type == WeightType.Random)
            {
                LinkAllLayersRandom();
            }

            if (type == WeightType.None)
            {
                LinkAllLayersNone();
            }


        }

        static public NeuralNetwork Mutation(NeuralNetwork baseNetwork, int strength)
        {
            int[] cloneCount = new int[baseNetwork.layers.Count];
            for (int i = 0; i < baseNetwork.layers.Count; i++)
            {
                cloneCount[i] = baseNetwork.layers[i].Count;
            }

            NeuralNetwork mutant = new NeuralNetwork(WeightType.None, cloneCount);

            List<NInput> allToRandom = new List<NInput>();

            for (int i = 0; i < baseNetwork.layers.Count - 1; i++)
            {
                for (int j = 0; j < baseNetwork.layers[i + 1].Count; j++)
                {
                    for (int k = 0; k < baseNetwork.layers[i].Count; k++)
                    {
                        mutant.layers[i + 1][j].inputs[k].weight = baseNetwork.layers[i + 1][j].inputs[k].weight;
                        allToRandom.Add(mutant.layers[i + 1][j].inputs[k]);
                    }
                }
            }

            Random rnd = new Random();

            for (int sw = 0; sw < strength; sw++)
            {
                int r = rnd.Next(0, allToRandom.Count);
                allToRandom[r].weight = (rnd.NextDouble() - 0.5) * 2;
            }

            return mutant;
        }

        static public NeuralNetwork[] Crossing(NeuralNetwork a, NeuralNetwork b, int strength)
        {
            Random rnd = new Random();
            int[] cloneCount = new int[a.layers.Count];

            for (int i = 0; i < a.layers.Count; i++)
            {
                cloneCount[i] = a.layers[i].Count;
            }

            int field = 0;

            for (int i = 0; i < cloneCount.Length - 1; i++)
            {
                field += cloneCount[i] * cloneCount[i + 1];
            }

            int[] points = new int[strength];

            for (int i = 0; i < strength; i++)
            {
                int r = rnd.Next(0, field + 1);
                bool t = false;
                while (t == false)
                {
                    t = true;
                    foreach (var item in points)
                    {
                        if (r == item)
                        {
                            t = false;
                            break;
                        }
                    }
                    if (t == false)
                    {
                        r = rnd.Next(0, field + 1);
                    }
                    else
                    {
                        points[i] = r;
                    }
                }
            }

            NeuralNetwork s1 = new NeuralNetwork(WeightType.None, cloneCount);
            NeuralNetwork s2 = new NeuralNetwork(WeightType.None, cloneCount);

            int counter = 0;
            bool state = false;

            for (int i = 0; i < a.layers.Count - 1; i++)
            {
                for (int j = 0; j < a.layers[i + 1].Count; j++)
                {
                    for (int k = 0; k < a.layers[i].Count; k++)
                    {
                        foreach (var item in points)
                        {
                            if (counter == item)
                            {
                                state = !state;
                                break;
                            }
                        }
                        if (state == false)
                        {
                            s1.layers[i + 1][j].inputs[k].weight = a.layers[i + 1][j].inputs[k].weight;
                            s2.layers[i + 1][j].inputs[k].weight = b.layers[i + 1][j].inputs[k].weight;
                        }
                        else
                        {
                            s2.layers[i + 1][j].inputs[k].weight = a.layers[i + 1][j].inputs[k].weight;
                            s1.layers[i + 1][j].inputs[k].weight = b.layers[i + 1][j].inputs[k].weight;
                        }
                        counter++;
                    }
                }
            }
            NeuralNetwork[] output = { s1, s2 };
            return output;
        }

        private void LinkAllLayersNone()
        {
            for (int k = 0; k < layers.Count - 1; k++)
            {
                LinkLayersRandom(layers[k + 1], layers[k]);
            }
        }

        private void LinkAllLayersRandom()
        {
            for (int k = 0; k < layers.Count - 1; k++)
            {
                LinkLayersRandom(layers[k + 1], layers[k]);
            }
        }

        static private void LinkLayersNone(List<Neuron> home, List<Neuron> end)
        {
            foreach (Neuron hn in home)
            {
                foreach (Neuron en in end)
                {
                    hn.inputs.Add(new NInput(en));
                }
            }
        }

        static private void LinkLayersRandom(List<Neuron> home, List<Neuron> end)
        {
            Random rnd = new Random();
            foreach (Neuron hn in home)
            {
                foreach (Neuron en in end)
                {
                    hn.inputs.Add(new NInput(rnd.NextDouble() - 0.5, en));
                }
            }
        }

        public double[] RunIteration(params double[] inputs)
        {
            int i = 0;

            for (int j = 0; j < layers[i].Count; j++)
            {
                layers[i][j].output = inputs[j];
            }
            i++;

            while (i < layers.Count)
            {
                foreach (Neuron neuron in layers[i])
                {
                    neuron.Activation();
                }
                i++;
            }

            double[] output = new double[layers[layers.Count - 1].Count];

            for (int j = 0; j < layers[layers.Count - 1].Count; j++)
            {
                output[j] = layers[layers.Count - 1][j].output;
            }

            return output;

        }
    }

    [Serializable]
    public class NInput
    {
        public double weight;
        Neuron neuron;

        public double GetWeighted()
        {
            return neuron.output * weight;
        }

        public NInput(double w, Neuron n)
        {
            weight = w;
            neuron = n;
        }
        public NInput(Neuron n)
        {
            neuron = n;
        }
        public NInput()
        {

        }

    }

    [Serializable]
    public class Neuron
    {
        public double output;
        public List<NInput> inputs = new List<NInput>();
        public NeuronType type = NeuronType.Hidden;

        public Neuron()
        {
           
        }

        public Neuron(NeuronType type)
        {
            this.type = type;
        }        

        private double GetAllWeighted()
        {
            double sum = 0;
            foreach (NInput input in inputs)
            {
                sum += input.GetWeighted();
            }
            return sum;
        }

        public void Activation()
        {
            output = 1 / (1 + Math.Exp(-2 * GetAllWeighted()));        
        }
    }
}
  