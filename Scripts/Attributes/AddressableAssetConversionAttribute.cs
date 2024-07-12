namespace MultiplayerARPG
{
    public class AddressableAssetConversionAttribute : System.Attribute
    {
        public string AddressableVarName { get; private set; }
        public string ConvertFunctionName { get; private set; }

        public AddressableAssetConversionAttribute(string addressableVarName, string convertFunctionName)
        {
            AddressableVarName = addressableVarName;
            ConvertFunctionName = convertFunctionName;
        }

        public AddressableAssetConversionAttribute(string addressableVarName)
            : this(addressableVarName, string.Empty)
        { 
        }
    }
}
