using Bunit;
using Maliev.ShadcnBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Maliev.ShadcnBlazor.Tests.Components.Forms;

public sealed class DropzoneTests : BunitContext
{
    public DropzoneTests() => Services.AddMalievShadcn();

    [Fact]
    public void StatusAndValidationMessagesCanBeLocalized()
    {
        var cut = Render<DynamicComponent>(parameters => parameters
            .Add(component => component.Type, typeof(ShadcnDropzone))
            .Add(component => component.Parameters, new Dictionary<string, object>
            {
                [nameof(ShadcnDropzone.Loading)] = true,
                ["LoadingStatus"] = "กำลังประมวลผลไฟล์ที่เลือก",
                ["SelectedFilesStatus"] = (Func<int, string>)(count => $"เลือกแล้ว {count} ไฟล์"),
                ["ValidationErrorsStatus"] = (Func<int, string>)(count => $"พบข้อผิดพลาด {count} รายการ")
            }));

        Assert.Equal("กำลังประมวลผลไฟล์ที่เลือก", cut.Find("[data-slot='dropzone-status']").TextContent.Trim());

        var overload = typeof(ShadcnDropzoneValidation).GetMethods()
            .SingleOrDefault(method => method.Name == nameof(ShadcnDropzoneValidation.Validate) && method.GetParameters().Length == 6);
        Assert.NotNull(overload);
        Func<ShadcnDropzoneErrorCode, string?, long, string> formatter = (code, fileName, limit) => $"{code}:{fileName}:{limit}";
        IBrowserFile[] files = [new TestBrowserFile("large.pdf", 5_000, "application/pdf")];
        var selection = Assert.IsType<ShadcnDropzoneSelection>(overload.Invoke(null, [files, null, false, 1, 1_000L, formatter]));
        Assert.Equal("FileTooLarge:large.pdf:1000", Assert.Single(selection.Errors).Message);
    }

    [Fact]
    public void RendersOneAccessibleNativeFileBoundaryForClickKeyboardAndDrop()
    {
        var cut = Render<ShadcnDropzone>(parameters => parameters
            .Add(component => component.Accept, ".step,.stp,application/pdf")
            .Add(component => component.Multiple, true)
            .Add(component => component.Instructions, "Drop drawings here or choose files")
            .Add(component => component.Description, "STEP or PDF, up to 20 MB each"));

        var root = cut.Find("[data-slot='dropzone']");
        var input = cut.Find("input[type='file']");
        Assert.Equal("group", root.GetAttribute("role"));
        Assert.Equal("idle", root.GetAttribute("data-state"));
        Assert.Equal(".step,.stp,application/pdf", input.GetAttribute("accept"));
        Assert.True(input.HasAttribute("multiple"));
        var instructionsId = cut.Find("[data-slot='dropzone-instructions']").Id;
        Assert.False(string.IsNullOrWhiteSpace(instructionsId));
        Assert.Contains(instructionsId!, input.GetAttribute("aria-describedby"), StringComparison.Ordinal);
        Assert.NotNull(cut.Find("[data-slot='dropzone-status'][role='status'][aria-live='polite']"));
    }

    [Fact]
    public void DisabledAndLoadingStatesDisableTheOnlyInteractiveBoundary()
    {
        var disabled = Render<ShadcnDropzone>(parameters => parameters.Add(component => component.Disabled, true));
        Assert.True(disabled.Find("input").HasAttribute("disabled"));
        Assert.Equal("disabled", disabled.Find("[data-slot='dropzone']").GetAttribute("data-state"));

        var loading = Render<ShadcnDropzone>(parameters => parameters.Add(component => component.Loading, true));
        Assert.True(loading.Find("input").HasAttribute("disabled"));
        Assert.Equal("loading", loading.Find("[data-slot='dropzone']").GetAttribute("data-state"));
        Assert.Equal("true", loading.Find("[data-slot='dropzone']").GetAttribute("aria-busy"));
    }

    [Fact]
    public void ValidatorReturnsStableCountSizeAndTypeErrorsWithoutUploading()
    {
        IBrowserFile[] files =
        [
            new TestBrowserFile("fixture.step", 512, "application/step"),
            new TestBrowserFile("oversize.pdf", 4097, "application/pdf"),
            new TestBrowserFile("notes.txt", 128, "text/plain")
        ];

        var selection = ShadcnDropzoneValidation.Validate(files, ".step,application/pdf", multiple: true, maxFiles: 2, maxFileSize: 4096);

        Assert.Same(files, selection.Files);
        Assert.Equal(
            [ShadcnDropzoneErrorCode.TooManyFiles, ShadcnDropzoneErrorCode.FileTooLarge, ShadcnDropzoneErrorCode.FileTypeNotAccepted],
            selection.Errors.Select(error => error.Code));
        Assert.All(selection.Errors, error => Assert.False(string.IsNullOrWhiteSpace(error.Message)));
    }

    [Fact]
    public void InvalidConfigurationFailsBeforeRendering()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnDropzone>(parameters => parameters.Add(component => component.MaxFiles, 0)));
        Assert.ThrowsAny<Exception>(() => Render<ShadcnDropzone>(parameters => parameters.Add(component => component.MaxFileSize, 0)));
    }

    private sealed record TestBrowserFile(string Name, long Size, string ContentType) : IBrowserFile
    {
        public DateTimeOffset LastModified { get; } = DateTimeOffset.UnixEpoch;
        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
