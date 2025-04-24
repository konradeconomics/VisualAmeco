namespace VisualAmeco.Core.Enums;

public enum AggregationMode
{
    StandardAggregations = 0,
    WeightedMeanNationalRatiosCurrentPricesECU = 1,
    WeightedMeanNationalRatiosCurrentPricesPPS = 2,
    WeightedGeometricMeanGDPCurrentECU = 3,
    WeightedGeometricMeanGDPCurrentPPS = 4,
    WeightedGeometricMeanPrivateConsumptionCurrentECU = 5,
    WeightedGeometricMeanPrivateConsumptionCurrentPPS = 6,
    WeightedGeometricMeanCompetitorGroupUsingExportWeights = 7
}