using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestBorges : MonoBehaviour
{
    [Header("Genetic Algorithm")]
    [SerializeField] string targetString = "Que el cielo exista, aunque nuestro lugar sea el infierno.";
    [SerializeField] string validCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ,.|!#$%&/()=? ";
    [SerializeField] int populationSize = 200;
    [SerializeField] float mutationRate = 0.01f;
    [SerializeField] int elitism = 5;

    [Header("Other")]
    [SerializeField] int numCharPerText = 15000;
    [SerializeField] Text targetText;
    [SerializeField] Text bestText;
    [SerializeField] Text bestFitnessText;
    [SerializeField] Text numGenerationText;
    [SerializeField] Transform populationTextParent;
    [SerializeField] Text textPrefab;

    private int numCharsPerTextObj;
	private List<Text> textList = new List<Text>();

    GeneticAlgorithm<char> ga;
    System.Random random;




    // Start is called before the first frame update
    void Start()
    {
        targetText.text = targetString;
        if (string.IsNullOrEmpty(targetString))
        {
            Debug.LogError("Target string is null or empty");
            this.enabled = false;
        }

        random = new System.Random();
        ga = new GeneticAlgorithm<char>(populationSize, targetString.Length, random, GetRandomCharacter, FitnessFunction, elitism, mutationRate);
    }

    // Update is called once per frame
    void Update()
    {
        ga.NewGeneration();
        UpdateText(ga.BestGenes, ga.BestFitness, ga.Generation, ga.Population.Count, (j) => ga.Population[j].Genes);

        if(ga.BestFitness == 1){
            this.enabled = false;
        }

    }

    void Awake()
	{
		numCharsPerTextObj = numCharPerText / validCharacters.Length;
		if (numCharsPerTextObj > populationSize) numCharsPerTextObj = populationSize;

		int numTextObjects = Mathf.CeilToInt((float)populationSize / numCharsPerTextObj);

		for (int i = 0; i < numTextObjects; i++)
		{
			textList.Add(Instantiate(textPrefab, populationTextParent));
		}
	}

    char GetRandomCharacter()
    {
        int i = random.Next(validCharacters.Length);
        return validCharacters[i];
    }

    float FitnessFunction(int index)
    {
        float score = 0;
        DNA<char> dna = ga.Population[index];
        for (int i = 0; i < dna.Genes.Length; i++)
        {
            if(dna.Genes[i] == targetString[i])
            {
                score +=1;
            }
        }

        score /= targetString.Length;
        int pow = 3;
        score = (Mathf.Pow(pow, score)-1)/(pow-1);
        return score;
    }

    private void UpdateText(char[] bestGenes, float bestFitness, int generation, int populationSize, Func<int, char[]> getGenes)
	{
		bestText.text = CharArrayToString(bestGenes);
		bestFitnessText.text = bestFitness.ToString();

		numGenerationText.text = generation.ToString();

		for (int i = 0; i < textList.Count; i++)
		{
			var sb = new StringBuilder();
			int endIndex = i == textList.Count - 1 ? populationSize : (i + 1) * numCharsPerTextObj;
			for (int j = i * numCharsPerTextObj; j < endIndex; j++)
			{
				foreach (var c in getGenes(j))
				{
					sb.Append(c);
				}
				if (j < endIndex - 1) sb.AppendLine();
			}

			textList[i].text = sb.ToString();
		}
	}

    private string CharArrayToString(char[] charArray)
	{
		var sb = new StringBuilder();
		foreach (var c in charArray)
		{
			sb.Append(c);
		}

		return sb.ToString();
	}
}
