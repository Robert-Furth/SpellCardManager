using SpellCardManager.Services;
using Win = System.Windows;

namespace SpellCardManager.WPFApp.Services;

class MessageBoxService : IMessageBoxService {
    public Task<MessageBoxResult> ShowErrorBox(
            string title,
            string message,
            MessageBoxButtons buttons = MessageBoxButtons.Ok) => ShowBoxInternal(
                title, message, buttons, Win.MessageBoxImage.Error);

    public Task<MessageBoxResult> ShowMessageBox(
        string title,
        string message,
        MessageBoxButtons buttons = MessageBoxButtons.Ok) => ShowBoxInternal(
            title, message, buttons, Win.MessageBoxImage.None);

    public Task<MessageBoxResult> ShowWarningBox(
        string title,
        string message,
        MessageBoxButtons buttons = MessageBoxButtons.Ok) => ShowBoxInternal(
            title, message, buttons, Win.MessageBoxImage.Warning);

    private static Task<MessageBoxResult> ShowBoxInternal(
        string title,
        string message,
        MessageBoxButtons buttons,
        Win.MessageBoxImage image) => Task.FromResult(FromWinResult(Win.MessageBox.Show(message, title, ToWinButton(buttons), image)));

    private static MessageBoxResult FromWinResult(Win.MessageBoxResult result) => result switch {
        Win.MessageBoxResult.Yes => MessageBoxResult.Yes,
        Win.MessageBoxResult.No => MessageBoxResult.No,
        Win.MessageBoxResult.OK => MessageBoxResult.Ok,
        Win.MessageBoxResult.Cancel => MessageBoxResult.Cancel,
        Win.MessageBoxResult.None => MessageBoxResult.None,
        _ => throw new ArgumentOutOfRangeException(nameof(result))
    };

    private static Win.MessageBoxButton ToWinButton(MessageBoxButtons buttons) => buttons switch {
        MessageBoxButtons.Ok => Win.MessageBoxButton.OK,
        MessageBoxButtons.YesNo => Win.MessageBoxButton.YesNo,
        MessageBoxButtons.YesNoCancel => Win.MessageBoxButton.YesNoCancel,
        _ => throw new ArgumentOutOfRangeException(nameof(buttons))
    };
}
