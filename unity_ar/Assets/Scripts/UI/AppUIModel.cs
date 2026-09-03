namespace ViMARA.UI
{
    // Modelo: Sólo datos y estado de la sesión, C# puro y limpio. No sabe de botones ni Unity UI.
    public class AppUIModel
    {
        public string SelectedFilePath { get; private set; }

        public void SetFilePath(string path)
        {
            SelectedFilePath = path;
        }

        public void ClearFilePath()
        {
            SelectedFilePath = string.Empty;
        }
        
        // TODO: A futuro aquí podrías agregar la lógica para guardar el Modo Seleccionado
        // (Por ejemplo: public bool SeleccionoModoPlano { get; set; })
    }
}
