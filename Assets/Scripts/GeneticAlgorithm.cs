using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting.Dependencies.Sqlite;

public class GeneticAlgorithm<T>
{
    public List<DNA<T>> Population { get; private set; }
    public int Generation { get; private set; }
    public float BestFitness { get; private set; }
    public T[] BestGenes { get; private set; }
    public float MutationRate;
    public int Elitism;
    private Random random;
    private float fitnessSum;
    List<DNA<T>> newPopulation;
    int dnaSize;
    Func<T> getRandomGene;
    Func<int, float> fitnessFunction;

    public GeneticAlgorithm(int populationSize, int dnaSize, Random random, Func<T> getRandomGene, Func<int, float> fitnessFunction, int elitism, float mutationRate = 0.01f)
    {
        Generation = 1;
        MutationRate = mutationRate;
        Population = new List<DNA<T>>(populationSize);
        newPopulation = new List<DNA<T>>(populationSize);
        this.random = random;
        Elitism = elitism;
        this.dnaSize = dnaSize;
        this.getRandomGene = getRandomGene;
        this.fitnessFunction = fitnessFunction;

        BestGenes = new T[dnaSize];

        for (int i = 0; i < populationSize; i++)
        {
            Population.Add(new DNA<T>(dnaSize, random, getRandomGene, fitnessFunction, shouldInitGenes: true));
        }
    }

    public void NewGeneration(int numNewDNA = 0, bool crossoverDNA = false)
    {
        int finalCount = Population.Count + numNewDNA;

        if (finalCount <= 0)
        {
            return;
        }

        if (Population.Count > 0)
        {
            CalculateFitness();
            Population.Sort(CompareDNA);
        }

        newPopulation.Clear();

        for (int i = 0; i < finalCount; i++)
        {
            if (i < Elitism && i < Population.Count)
            {
                newPopulation.Add(Population[i]);
            }
            else
            {
                if (i < Population.Count || crossoverDNA)
                {
                    DNA<T> parent1 = ChooseParent();
                DNA<T> parent2 = ChooseParent();

                DNA<T> child = parent1.Crossover(parent2);
                child.Mutate(MutationRate);
                newPopulation.Add(child);
                } else {
                    newPopulation.Add(new DNA<T>(dnaSize, random, getRandomGene, fitnessFunction, shouldInitGenes: true));
                }
                
            }

        }
        List<DNA<T>> tmpPopulation = Population;
        Population = newPopulation;
        newPopulation = tmpPopulation;
        Generation++;
    }

    public int CompareDNA(DNA<T> a, DNA<T> b)
    {
        if (a.Fitness == b.Fitness)
        {
            return 0;
        }

        if (a.Fitness > b.Fitness)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }

    public void CalculateFitness()
    {
        DNA<T> best = Population[0];
        fitnessSum = 0;
        for (int i = 0; i < Population.Count; i++)
        {
            fitnessSum += Population[i].CalculateFitness(i);

            if (Population[i].Fitness > best.Fitness)
            {
                best = Population[i];
            }
        }
        BestFitness = best.Fitness;
        best.Genes.CopyTo(BestGenes, 0);
    }

    DNA<T> ChooseParent()
    {
        double randomNumber = random.NextDouble() * fitnessSum;

        for (int i = 0; i < Population.Count; i++)
        {
            if (randomNumber < Population[i].Fitness)
            {
                return Population[i];
            }
            randomNumber -= Population[i].Fitness;
        }
        return null;
    }
}
