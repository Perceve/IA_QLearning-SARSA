using UnityEngine;
using System.Collections;

public class QLearning : MonoBehaviour
{
    [SerializeField] private GameBoard gameBoard;
    [SerializeField] private PlayerController playerController;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float discountRate = 1.0f;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float learningRate = 1.0f;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float epsilon = 0.05f;

    [SerializeField] private float waitTime = 0.1f;

    [SerializeField] private int rewardPerMovement = -1;
    [SerializeField] private int rewardForReachingGoal = 100;
    [SerializeField] private int rewardForNotMoving = -100;
    [SerializeField] private int rewardForCoin = 10;
    [SerializeField] private int rewardForDying = -1000;

    private float[,] stateActions;

    //Iniciamos el bucle de selecci�n
    void Awake()
    {
        stateActions = new float[gameBoard.cells.Length, 4];
        StartCoroutine(QLoop());
    }

    private IEnumerator QLoop()
    {
        

        while (true)
        {
            //Guardamos el estado actual
            int currentState = playerController.currentCell;

            //Encontramos una acci�n con la pol�tica de selecci�n epsilon-greedy
            int bestCurrentAction = EpsilonFGreedySelection(currentState);

            //Movemos al agente en esa direcci�n
            playerController.Move((GameBoard.Directions)bestCurrentAction);

            //Guardamos el estado resultante de dicha acci�n
            int newState = playerController.currentCell;

            //Evaluamos la recompensa de la acci�n tomada
            int reward = Reward(currentState, newState);

            //Guardamos una valoraci�n en el estado-acci�n elegido en funci�n de la mejor acci�n posible al que hemos llegado
            stateActions[currentState, bestCurrentAction] += learningRate * (reward + discountRate * stateActions[newState, BestActionForState(newState)]) - stateActions[currentState, bestCurrentAction];

            yield return new WaitForSeconds(waitTime);
        }

    }

    //Devuelve una recompensa en funci�n del movimiento hecho
    private int Reward(int initialState, int endState)
    {
        if (endState == gameBoard.endCell)
        {
            playerController.ResetPlayer();
            return rewardForReachingGoal;
        }
        else if (initialState == endState)
        {
            return rewardForNotMoving;
        }
        else if (gameBoard.CellHasCoin(endState))
        {
            return rewardForCoin;
        }
        else if (gameBoard.CellHasEnemy(endState))
        {
            return rewardForDying;
        }
        else
        {
            return rewardPerMovement;
        }
    }

    //Escoge entre coger la mejor opci�n posible o coger uan acci�n aleatoria para favorecer la exploraci�n. Lo hacer en funci�n de epsilon, que deberia favorecer coger la mejor opci�n la mayora de las veces
    private int EpsilonFGreedySelection(int state)
    {
        float randomFloat = Random.value;

        if (randomFloat > epsilon)
        {
            return BestActionForState(state);
        }
        else
        {
            return Random.Range(0, 4);
        }
    }

    //Devuelve la mejor acci�n para un estado
    private int BestActionForState(int state)
    {
        float bestScore = stateActions[state, 0];
        int bestAction = 0;

        for (int i = 0; i < 4; i++)
        {
            float currentScore = stateActions[state, i];
            if (currentScore > bestScore)
            {
                bestScore = currentScore;
                bestAction = i;
            }
        }

        return bestAction;
    }
}
