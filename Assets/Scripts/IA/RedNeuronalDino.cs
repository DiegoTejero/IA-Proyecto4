using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedNeuronalDino : MonoBehaviour
{

    private RedNeuronal network;
    public int numInput = 5, numOutput = 3, numHidden = 4;
    // INPUTS: Distancia a obstaculo, altura del dino, altura del obstaculo, espacio entre obstaculos, velocidad
    //OUTPUTS: 0 = saltar, 1 = agacharse, 2 = bajar
    private static float[,] training =
                {
                {10, 0.5f, 0.32f, 20, 0.3f,                    0.9f, 0.1f, 0.1f},
                {5, 0.5f, 0.32f, 20, 0.05f,                    0.9f, 0.1f, 0.1f},
                {10, 0.88f, 0.5f, 20, 0.3f,                    0.9f, 0.1f, 0.1f},
                {10, 0.88f, 0.5f, 20, 0,                       0.9f, 0.1f, 0.1f},
                {8, 0.88f, 1.2f, 15, 0.3f,                     0.9f, 0.1f, 0.1f},
                {8, 0.88f, 1.53f, 15, 0.3f,                    0.1f, 0.9f, 0.1f},
                {20, 1.2f, 1.2f, 15, 0.3f,                     0.1f, 0.1f, 0.9f},
                {15, 0.88f, 0.32f, 15, 0.3f,                   0.1f, 0.1f, 0.9f},
                {15, 0.5f, 0.32f, 15, 0.3f,                    0.1f, 0.1f, 0.9f},
                {20, 0.88f, 0.32f, 15, 0f,                     0.1f, 0.1f, 0.9f},
                {4 ,1.8f, 1.8f, 15, 0.3f,                      0.1f, 0.1f, 0.9f},
                {6 ,1.53f, 1.53f, 10, 0.3f,                    0.1f, 0.1f, 0.9f},
                {8, 1.3f, 0.5f, 8, 0.3f,                       0.1f, 0.1f, 0.1f},
                {8, 1.3f, 0.5f, 5, 0,                          0.1f, 0.1f, 0.1f},
                {4, 0.88f, 1.8f, 10, 0.3f,                     0.1f, 0.9f, 0.1f},
        };

    public static RedNeuronalDino instance;

    // Use this for initialization
    void Start()
    {

        instance = this;
        network = new RedNeuronal(numInput, numHidden, numOutput);
        TrainNetwork();
    }

    private void TrainNetwork()
    {
        float error = 1;
        int epoch = 0;

        while ((error > 0.05f) && (epoch < 50000))
        {

            error = 0;
            epoch++;

            for (int i = 0; i < training.GetLength(0); i++)
            {
                for (int j = 0; j < numInput; j++)
                {
                    network.SetInput(j, training[i, j]);
                }

                for (int j = numInput; j < numInput + numOutput; j++)
                {
                    network.SetOutputDeseado(j - numInput, training[i, j]);
                }
                network.FeedForward();
                error += network.CalcularError();
                network.BackPropagation();
            }
            error = error / training.GetLength(0);
        }

    }

    public void RetrainNetwork(float[] inputs, float[] outputs)
    {
        float error = 1;
        int epoch = 0;

        while ((error > 0.1f) && (epoch < 1000))
        {

            epoch++;

            for (int j = 0; j < numInput; j++)
            {
                network.SetInput(j, inputs[j]);
            }

            for (int j = 0; j < numOutput; j++)
            {
                network.SetOutputDeseado(j, outputs[j]);
            }
            network.FeedForward();
            error = network.CalcularError();
            network.BackPropagation();

        }

    }

    public byte CheckAction(float[] inputs)
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            network.SetInput(i, inputs[i]);
        }
        network.FeedForward();
        return (byte)network.GetMaxOutputId();
    }

    public float GetOutput(int i)
    {
        return network.GetOutput(i);
    }
    public int GetMaxOutputId()
    {
        return network.GetMaxOutputId();
    }
}


