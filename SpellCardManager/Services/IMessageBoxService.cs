namespace SpellCardManager.Services;

public interface IMessageBoxService {
    public Task<MessageBoxResult> ShowMessageBox(
        string title,
        string message,
        MessageBoxButtons buttons = MessageBoxButtons.Ok);

    public Task<MessageBoxResult> ShowWarningBox(
        string title,
        string message,
        MessageBoxButtons buttons = MessageBoxButtons.Ok);

    public Task<MessageBoxResult> ShowErrorBox(
        string title,
        string message,
        MessageBoxButtons buttons = MessageBoxButtons.Ok);
}

public enum MessageBoxButtons {
    Ok, YesNo, YesNoCancel,
}

public enum MessageBoxResult {
    Yes, No, Ok, Cancel, None
}
