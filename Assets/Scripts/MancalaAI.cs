using UnityEngine;
using Unity.InferenceEngine;

public class MancalaAI : MonoBehaviour
{
    [SerializeField]
    private ModelAsset onnxModelAsset;
    private Model runtimeModel;
    private Worker worker;

    void Awake()
    {
        runtimeModel = ModelLoader.Load(onnxModelAsset);
        worker = new Worker(runtimeModel, BackendType.GPUCompute);
    }

    public int GetAIMove(float[] boardState, float[] validMovesMask)
    {
        var input = new Tensor<float>(new TensorShape(1, boardState.Length), boardState);
        worker.Schedule(input);
        var output = worker.PeekOutput() as Tensor<float>;

        var outputCpu = output.ReadbackAndClone() as Tensor<float>;
        float[] qValues = outputCpu.AsReadOnlyNativeArray().ToArray();

        for (int i = 0; i < qValues.Length; ++i)
        {
            if (validMovesMask[i] < 0.5f)
                qValues[i] = float.NegativeInfinity;
        }

        int bestMove = ArgMax(qValues);
        input.Dispose();
        output.Dispose();
        outputCpu.Dispose();
        return bestMove;
    }

    int ArgMax(float[] array)
    {
        int maxIndex = 0;
        for (int i = 1; i < array.Length; i++)
            if (array[i] > array[maxIndex])
                maxIndex = i;
        return maxIndex;
    }

    void OnDestroy()
    {
        worker?.Dispose();
    }
}