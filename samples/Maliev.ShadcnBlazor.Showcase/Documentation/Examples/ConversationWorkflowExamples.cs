using Maliev.ShadcnBlazor.Components.Conversation;
using Maliev.ShadcnBlazor.Components.Feedback;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

internal static class ConversationWorkflowExamples
{
    private sealed record AvatarProfile(string Id, string Source, string Alt);

    private static readonly AvatarProfile OperatorAvatar = new("operator", "images/avatars/operator-thai.png", "นที · วิศวกร");
    private static readonly AvatarProfile ReviewerAvatar = new("reviewer", "images/avatars/reviewer-thai.png", "ผู้ตรวจสอบคุณภาพ");
    private static readonly AvatarProfile AssistantAvatar = new("assistant", "images/avatars/assistant-thai.png", "MALIEV Assistant");
    private static readonly AvatarProfile CoordinatorAvatar = new("coordinator", "images/avatars/coordinator-thai.png", "มาลี · ผู้ประสานงาน");
    private static readonly AvatarProfile PlaceholderAvatar = new("placeholder", string.Empty, "ผู้ประสานงาน");

    public static IReadOnlyList<ComponentExampleDefinition> Create(string slug) => slug switch
    {
        "attachment" => [Attachment()],
        "bubble" => [Bubble()],
        "marker" => [Marker()],
        "message" => [Message()],
        "message-scroller" => [Scroller()],
        "questionnaire" => [Questionnaire()],
        _ => []
    };

    private static ComponentExampleDefinition Attachment()
    {
        const ShadcnAttachmentState state = ShadcnAttachmentState.Uploading;
        var vertical = false;
        var image = true;

        RenderFragment preview = b =>
        {
            b.OpenElement(0, "div");
            b.AddAttribute(1, "class", "showcase-attachment-demo");
            b.OpenComponent<ShadcnAttachmentGroup>(2);
            b.AddAttribute(3, nameof(ShadcnAttachmentGroup.AccessibleName), "Uploaded files");
            b.AddAttribute(4, nameof(ShadcnAttachmentGroup.Class), $"showcase-attachment-gallery{(vertical ? " showcase-attachment-gallery--vertical" : string.Empty)}");
            b.AddAttribute(5, nameof(ShadcnAttachmentGroup.ChildContent), (RenderFragment)(gallery =>
            {
                AddImageAttachment(gallery, 0, "workspace-plan.png", "PNG · 2.0 MB", "workspace", image);
                AddImageAttachment(gallery, 10, "desk-reference.png", "PNG · 2.0 MB", "desk", image);
                AddImageAttachment(gallery, 20, "office-reference.png", "PNG · 1.8 MB", "office", image);
            }));
            b.CloseComponent();

            b.OpenElement(6, "div");
            b.AddAttribute(7, "class", "showcase-attachment-files");
            AddFileAttachment(b, 10, "sales-dashboard.pdf", "PDF · 2.4 MB", state, state == ShadcnAttachmentState.Uploading ? 64 : null, vertical);
            AddFileAttachment(b, 20, "message-renderer.tsx", "TypeScript · 12 KB", ShadcnAttachmentState.Done, null, vertical);
            b.CloseElement();
            b.CloseElement();
        };

        return Example(
            "attachment",
            "Attachment lifecycle",
            preview,
            [
                Toggle("attachment-vertical", "Vertical", v => vertical = v),
                Toggle("attachment-image", "Image media", v => image = v, true)
            ],
            ["gallery", "image", "file", "uploading", "processing", "error", "done", "progress", "actions", "group", "rtl"],
            AttachmentRazorSource);
    }

    private static ComponentExampleDefinition Bubble()
    {
        var sentVariant = ShadcnBubbleVariant.Default;
        var receivedVariant = ShadcnBubbleVariant.Ghost;
        var end = false;
        var top = false;

        RenderFragment preview = b =>
        {
            b.OpenComponent<ConversationBubbleDossierPreview>(0);
            b.AddAttribute(1, nameof(ConversationBubbleDossierPreview.SentVariant), sentVariant);
            b.AddAttribute(2, nameof(ConversationBubbleDossierPreview.ReceivedVariant), receivedVariant);
            b.AddAttribute(3, nameof(ConversationBubbleDossierPreview.AlignIncomingEnd), end);
            b.AddAttribute(4, nameof(ConversationBubbleDossierPreview.ReactionsTop), top);
            b.CloseComponent();
        };

        return Example(
            "bubble",
            "Conversation bubble",
            preview,
            [
                Select("bubble-variant", "Sent variant", "Default", Enum.GetNames<ShadcnBubbleVariant>(), v => sentVariant = Enum.Parse<ShadcnBubbleVariant>(v)),
                Select("bubble-received-variant", "Received variant", "Ghost", Enum.GetNames<ShadcnBubbleVariant>(), v => receivedVariant = Enum.Parse<ShadcnBubbleVariant>(v)),
                Toggle("bubble-end", "Align incoming end", v => end = v),
                Toggle("bubble-reactions-top", "Reactions top", v => top = v)
            ],
            ["thread", "variants", "alignment", "reactions", "button", "link", "collapsible", "rtl"],
            BubbleRazorSource);
    }

    private static void AddImageAttachment(RenderTreeBuilder b, int sequence, string title, string description, string artwork, bool image)
    {
        b.OpenComponent<ShadcnAttachment>(sequence);
        b.AddAttribute(sequence + 1, nameof(ShadcnAttachment.State), ShadcnAttachmentState.Done);
        b.AddAttribute(sequence + 2, nameof(ShadcnAttachment.Orientation), ShadcnAttachmentOrientation.Vertical);
        b.AddAttribute(sequence + 3, nameof(ShadcnAttachment.Class), "showcase-attachment-card");
        b.AddAttribute(sequence + 4, nameof(ShadcnAttachment.Title), title);
        b.AddAttribute(sequence + 5, nameof(ShadcnAttachment.ChildContent), (RenderFragment)(content =>
        {
            content.OpenComponent<ShadcnAttachmentMedia>(0);
            content.AddAttribute(1, nameof(ShadcnAttachmentMedia.Variant), image ? ShadcnAttachmentMediaVariant.Image : ShadcnAttachmentMediaVariant.Icon);
            content.AddAttribute(2, nameof(ShadcnAttachmentMedia.ImageAlt), image ? $"Preview of {title}" : null);
            content.AddAttribute(3, nameof(ShadcnAttachmentMedia.Class), "showcase-attachment-thumbnail");
            content.AddAttribute(4, nameof(ShadcnAttachmentMedia.ChildContent), image ? Thumbnail(artwork) : FileIcon());
            content.CloseComponent();
            content.OpenComponent<ShadcnAttachmentContent>(5);
            content.AddAttribute(6, nameof(ShadcnAttachmentContent.ChildContent), (RenderFragment)(meta =>
            {
                AddText<ShadcnAttachmentTitle>(meta, 0, title);
                AddText<ShadcnAttachmentDescription>(meta, 3, description);
            }));
            content.CloseComponent();
        }));
        b.CloseComponent();
    }

    private static void AddFileAttachment(RenderTreeBuilder b, int sequence, string title, string description, ShadcnAttachmentState state, double? progress, bool vertical)
    {
        b.OpenComponent<ShadcnAttachment>(sequence);
        b.AddAttribute(sequence + 1, nameof(ShadcnAttachment.State), state);
        b.AddAttribute(sequence + 2, nameof(ShadcnAttachment.Progress), progress);
        b.AddAttribute(sequence + 3, nameof(ShadcnAttachment.Title), title);
        b.AddAttribute(sequence + 4, nameof(ShadcnAttachment.File), AttachmentFile(title, description));
        b.AddAttribute(sequence + 5, nameof(ShadcnAttachment.ErrorReason), state == ShadcnAttachmentState.Error ? "Upload failed" : null);
        b.AddAttribute(sequence + 6, nameof(ShadcnAttachment.Class), $"showcase-attachment-file{(vertical ? " showcase-attachment-file--vertical" : string.Empty)}");
        b.AddAttribute(sequence + 7, nameof(ShadcnAttachment.ChildContent), (RenderFragment)(content =>
        {
            content.OpenComponent<ShadcnAttachmentMedia>(0);
            content.AddAttribute(1, nameof(ShadcnAttachmentMedia.Class), "showcase-attachment-file-icon");
            content.AddAttribute(2, nameof(ShadcnAttachmentMedia.ChildContent), state == ShadcnAttachmentState.Uploading ? UploadSpinner() : FileIcon());
            content.CloseComponent();
            content.OpenComponent<ShadcnAttachmentContent>(3);
            content.AddAttribute(4, nameof(ShadcnAttachmentContent.ChildContent), (RenderFragment)(meta =>
            {
                AddText<ShadcnAttachmentTitle>(meta, 0, title);
                AddText<ShadcnAttachmentDescription>(meta, 3, AttachmentDescription(state, description, progress));
            }));
            content.CloseComponent();
            content.OpenComponent<ShadcnAttachmentActions>(6);
            content.AddAttribute(7, nameof(ShadcnAttachmentActions.ChildContent), (RenderFragment)(actions =>
            {
                actions.OpenComponent<ShadcnAttachmentAction>(0);
                actions.AddAttribute(1, nameof(ShadcnAttachmentAction.Action), state == ShadcnAttachmentState.Error ? ShadcnAttachmentActionKind.Retry : ShadcnAttachmentActionKind.Remove);
                actions.AddAttribute(2, nameof(ShadcnAttachmentAction.AccessibleName), state == ShadcnAttachmentState.Error ? $"Retry {title}" : $"Remove {title}");
                actions.AddAttribute(3, nameof(ShadcnAttachmentAction.ChildContent), CloseIcon());
                actions.CloseComponent();
            }));
            content.CloseComponent();
        }));
        b.CloseComponent();
    }

    private static ShadcnAttachmentFile AttachmentFile(string title, string description)
    {
        var contentType = title.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            ? "application/pdf"
            : "text/typescript";
        var size = description.Contains("2.4 MB", StringComparison.Ordinal) ? 2_400_000L : 12_000L;
        return new ShadcnAttachmentFile(title, size, contentType);
    }

    private static RenderFragment Thumbnail(string artwork) => b =>
    {
        var file = artwork switch
        {
            "workspace" => "workspace-plan.png",
            "desk" => "desk-reference.png",
            _ => "office-reference.png"
        };
        b.AddMarkupContent(0, $"<img class=\"showcase-attachment-artwork\" src=\"images/attachments/{file}\" alt=\"\" loading=\"lazy\" decoding=\"async\" />");
    };

    private static RenderFragment FileIcon() => b => b.AddMarkupContent(0, "<svg viewBox=\"0 0 24 24\" aria-hidden=\"true\" focusable=\"false\"><path d=\"M6 2.75h8l4 4v14.5H6z\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.6\"/><path d=\"M14 2.75v4h4M9 12h6M9 16h4\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.6\" stroke-linecap=\"round\"/></svg>");
    private static RenderFragment CloseIcon() => b => b.AddMarkupContent(0, "<svg viewBox=\"0 0 24 24\" aria-hidden=\"true\" focusable=\"false\"><path d=\"m7 7 10 10M17 7 7 17\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\"/></svg>");
    private static ComponentExampleDefinition Marker()
    {
        var variant = ShadcnMarkerVariant.Default;
        var streaming = true;
        RenderFragment preview = b =>
        {
            b.OpenElement(0, "div");
            b.AddAttribute(1, "class", "showcase-marker-thread showcase-bubble-thread");
            AddMessage(b, 2, ShadcnLogicalAlign.Start, AssistantAvatar, false, "MALIEV Assistant", "Machining is complete. Bore 4 is ready for final inspection.", bubbleVariant: ShadcnBubbleVariant.Ghost);
            AddMessage(b, 12, ShadcnLogicalAlign.End, OperatorAvatar, false, "Narin S.", "Probe result received. I am uploading the signed report now.", bubbleVariant: ShadcnBubbleVariant.Default);
            AddMessage(b, 22, ShadcnLogicalAlign.Start, ReviewerAvatar, false, "Kanda T.", "Thanks. I will hold dispatch until Quality signs off.", bubbleVariant: ShadcnBubbleVariant.Ghost);
            AddMarker(b, 32, variant, false, "✓", "Four inspection files verified");
            AddMarker(b, 42, ShadcnMarkerVariant.Separator, false, "•", "14:32 · WO-2418");
            AddMarker(b, 52, ShadcnMarkerVariant.Border, streaming, "✦", streaming ? "Preparing quality handoff" : "Ready for quality review");
            b.CloseElement();
        };
        return Example("marker", "Conversation marker", preview, [Select("marker-variant", "Primary variant", "Default", Enum.GetNames<ShadcnMarkerVariant>(), v => variant = Enum.Parse<ShadcnMarkerVariant>(v)), Toggle("marker-streaming", "Streaming status", v => streaming = v, true)], ["status", "separator", "border", "icon", "avatar", "streaming", "shimmer", "reduced-motion"], MarkerRazorSource);
    }

    private static ComponentExampleDefinition Message()
    {
        var end = false;
        var avatar = true;
        var footer = true;
        var footerAlways = false;
        RenderFragment preview = b =>
        {
            b.OpenComponent<ConversationMessageDossierPreview>(0);
            b.AddAttribute(1, nameof(ConversationMessageDossierPreview.AlignMiddleRowEnd), end);
            b.AddAttribute(2, nameof(ConversationMessageDossierPreview.Avatars), avatar);
            b.AddAttribute(3, nameof(ConversationMessageDossierPreview.FooterActions), footer);
            b.AddAttribute(4, nameof(ConversationMessageDossierPreview.AlwaysShowActions), footerAlways);
            b.CloseComponent();
        };
        return Example("message", "Message row", preview, [Toggle("message-end", "Align middle row end", v => end = v), Toggle("message-avatar", "Avatars", v => avatar = v, true), Toggle("message-footer", "Footer actions", v => footer = v, true), Toggle("message-footer-always", "Always show actions", v => footerAlways = v)], ["group", "start", "end", "avatar", "header", "footer", "copy", "reply", "quote", "hover-actions", "bubbles", "rtl"])
            with
        {
            RazorSourceProvider = () => MessageRazorSource(end, avatar, footer, footerAlways)
        };
    }

    private static void AddMarker(RenderTreeBuilder b, int sequence, ShadcnMarkerVariant variant, bool live, string icon, string text)
    {
        b.OpenComponent<ShadcnMarker>(sequence);
        b.AddAttribute(sequence + 1, nameof(ShadcnMarker.Variant), variant);
        b.AddAttribute(sequence + 2, nameof(ShadcnMarker.Live), live);
        b.AddAttribute(sequence + 3, nameof(ShadcnMarker.ChildContent), (RenderFragment)(content =>
        {
            content.OpenComponent<ShadcnMarkerIcon>(0);
            content.AddAttribute(1, nameof(ShadcnMarkerIcon.ChildContent), MarkerIconContent(live, icon));
            content.CloseComponent();
            content.OpenComponent<ShadcnMarkerContent>(3);
            content.AddAttribute(4, nameof(ShadcnMarkerContent.Streaming), live);
            content.AddAttribute(5, nameof(ShadcnMarkerContent.ChildContent), Text(text));
            content.AddAttribute(6, "dir", "auto");
            content.CloseComponent();
        }));
        b.CloseComponent();
    }

    private static void AddMessage(RenderTreeBuilder b, int sequence, ShadcnLogicalAlign align, AvatarProfile? avatar, bool footer, string author, string message, bool footerAlways = false, ShadcnBubbleVariant? bubbleVariant = null)
    {
        b.OpenComponent<ShadcnMessage>(sequence);
        b.AddAttribute(sequence + 1, nameof(ShadcnMessage.Align), align);
        b.AddAttribute(sequence + 2, nameof(ShadcnMessage.ChildContent), (RenderFragment)(row =>
        {
            AddAvatar(row, 0, avatar ?? PlaceholderAvatar);
            row.OpenComponent<ShadcnMessageContent>(3);
            row.AddAttribute(4, nameof(ShadcnMessageContent.ChildContent), (RenderFragment)(content =>
            {
                AddText<ShadcnMessageHeader>(content, 0, author);
                content.OpenComponent<ShadcnBubble>(3);
                content.AddAttribute(4, nameof(ShadcnBubble.Variant), bubbleVariant ?? (align == ShadcnLogicalAlign.End ? ShadcnBubbleVariant.Default : ShadcnBubbleVariant.Muted));
                content.AddAttribute(5, nameof(ShadcnBubble.Align), align);
                content.AddAttribute(6, nameof(ShadcnBubble.ChildContent), (RenderFragment)(bubble => AddText<ShadcnBubbleContent>(bubble, 0, message)));
                content.CloseComponent();
                if (footer)
                {
                    content.OpenComponent<ShadcnMessageFooter>(8);
                    content.AddAttribute(9, nameof(ShadcnMessageFooter.ChildContent), (RenderFragment)(actions =>
                    {
                        if (align == ShadcnLogicalAlign.End)
                        {
                            actions.OpenElement(0, "span");
                            actions.AddAttribute(1, "class", "showcase-message-status");
                            actions.AddAttribute(2, "aria-label", "ส่งแล้ว · 10:42");
                            actions.AddContent(3, "ส่งแล้ว · 10:42");
                            actions.CloseElement();
                        }
                        else
                        {
                            actions.OpenElement(0, "button");
                            actions.AddAttribute(1, "type", "button");
                            actions.AddAttribute(2, "class", "shadcn-message-action");
                            actions.AddAttribute(3, "aria-label", "ตอบกลับข้อความ");
                            actions.AddMarkupContent(4, ReplyIconMarkup());
                            actions.CloseElement();
                        }
                    }));
                    if (footerAlways)
                        content.AddAttribute(10, "data-visibility", "always");
                    content.CloseComponent();
                }
            }));
            row.CloseComponent();
        }));
        b.CloseComponent();
    }

    private static ComponentExampleDefinition Scroller()
    {
        var auto = true; var extra = false; var position = ShadcnMessageDefaultScrollPosition.End;
        RenderFragment preview = b =>
        {
            b.OpenComponent<StreamingScrollerDemo>(0);
            b.AddAttribute(1, nameof(StreamingScrollerDemo.AutoFollow), auto);
            b.AddAttribute(2, nameof(StreamingScrollerDemo.AppendUnread), extra);
            b.AddAttribute(3, nameof(StreamingScrollerDemo.OpeningPosition), position);
            b.CloseComponent();
        };
        return Example("message-scroller", "Streaming transcript", preview, [Toggle("scroller-auto", "Auto follow", v => auto = v, true), Toggle("scroller-append", "Append unread turn", v => extra = v), Select("scroller-position", "Opening position", "End", Enum.GetNames<ShadcnMessageDefaultScrollPosition>(), v => position = Enum.Parse<ShadcnMessageDefaultScrollPosition>(v))], ["anchor", "auto-follow", "user-intent", "unread", "jump", "prepend", "visibility", "focus", "rtl"], MessageScrollerRazorSource);
    }

    private static ComponentExampleDefinition Questionnaire()
    {
        var branch = true; var start = "scope";
        RenderFragment preview = b =>
        {
            var items = branch
                ? new[] { new ShadcnQuestionnaireItemDefinition("scope", Required: true, AllowsFreeform: true, Choices: [new("component"), new("feature"), new("other", Custom: true)]), new ShadcnQuestionnaireItemDefinition("notes", AllowsFreeform: true) }
                : [new ShadcnQuestionnaireItemDefinition("scope", Required: true, AllowsFreeform: true, Choices: [new("component"), new("feature"), new("other", Custom: true)])];
            b.OpenComponent<ShadcnQuestionnaire>(0);
            b.SetKey($"{branch}-{start}");
            b.AddAttribute(1, nameof(ShadcnQuestionnaire.Items), items);
            b.AddAttribute(2, nameof(ShadcnQuestionnaire.DefaultItem), start == "notes" && branch ? "notes" : "scope");
            b.AddAttribute(3, nameof(ShadcnQuestionnaire.AccessibleName), "ขอบเขตงาน");
            b.AddAttribute(4, nameof(ShadcnQuestionnaire.Shortcuts), ShadcnQuestionnaireShortcutMode.Numbers);
            b.AddAttribute(5, "class", "showcase-questionnaire-card");
            b.AddAttribute(6, nameof(ShadcnQuestionnaire.ChildContent), (RenderFragment)(questionnaire =>
            {
                questionnaire.OpenComponent<ShadcnQuestionnaireProgress>(0);
                questionnaire.AddAttribute(1, nameof(ShadcnQuestionnaireProgress.AccessibleName), "ความคืบหน้า");
                questionnaire.CloseComponent();
                AddQuestion(questionnaire, 3, "scope", "เลือกประเภทการตรวจ", "เลือก workflow ที่ต้องการสาธิต", false);
                if (branch)
                    AddQuestion(questionnaire, 10, "notes", "รายละเอียดเพิ่มเติม", "อธิบายสิ่งที่ต้องการให้ทีมตรวจสอบ", true);
                questionnaire.OpenComponent<ShadcnQuestionnaireActions>(20);
                questionnaire.AddAttribute(21, nameof(ShadcnQuestionnaireActions.ChildContent), (RenderFragment)(actions =>
                {
                    AddText<ShadcnQuestionnairePrevious>(actions, 0, "ก่อนหน้า");
                    AddText<ShadcnQuestionnaireSkip>(actions, 3, "ข้าม");
                    AddText<ShadcnQuestionnaireNext>(actions, 6, "ถัดไป");
                    AddText<ShadcnQuestionnaireSubmit>(actions, 9, "ส่งคำตอบ");
                }));
                questionnaire.CloseComponent();
            }));
            b.CloseComponent();
        };
        return Example("questionnaire", "Guided questionnaire", preview, [Toggle("questionnaire-branch", "Conditional notes", v => branch = v, true), Select("questionnaire-start", "Resume item", "scope", ["scope", "notes"], v => start = v)], ["single", "multiple", "freeform", "custom", "skipped", "required", "invalid", "controlled", "resume", "branching", "submit", "thai", "english", "rtl"])
            with
        { RazorSourceProvider = () => QuestionnaireRazorSource(branch, start) };
    }

    private static void AddQuestion(RenderTreeBuilder b, int s, string name, string title, string description, bool input)
    {
        b.OpenComponent<ShadcnQuestionnaireItem>(s);
        b.AddAttribute(s + 1, nameof(ShadcnQuestionnaireItem.Name), name);
        b.AddAttribute(s + 2, nameof(ShadcnQuestionnaireItem.ChildContent), (RenderFragment)(x =>
        {
            AddText<ShadcnQuestionnaireTitle>(x, 0, title);
            AddText<ShadcnQuestionnaireDescription>(x, 3, description);
            if (input)
            {
                x.OpenComponent<ShadcnQuestionnaireInput>(6);
                x.AddAttribute(7, nameof(ShadcnQuestionnaireInput.AccessibleName), title);
                x.AddAttribute(8, "placeholder", "พิมพ์คำตอบของคุณ");
                x.CloseComponent();
            }
            else
            {
                x.OpenComponent<ShadcnQuestionnaireChoices>(6);
                x.AddAttribute(7, nameof(ShadcnQuestionnaireChoices.ChildContent), (RenderFragment)(choices =>
                {
                    AddChoice(choices, 0, "component", "Component", "ตัวอย่างองค์ประกอบ UI");
                    AddChoice(choices, 3, "feature", "Feature", "workflow หรือฟีเจอร์ใหม่");
                    AddChoice(choices, 6, "other", "อื่น ๆ · Other", "ระบุประเภทงานด้วยตนเอง · Describe it yourself");
                }));
                x.CloseComponent();
                x.OpenComponent<ShadcnQuestionnaireInput>(9);
                x.AddAttribute(10, nameof(ShadcnQuestionnaireInput.AccessibleName), "ระบุประเภทงาน · Describe the work");
                x.AddAttribute(11, "placeholder", "พิมพ์คำตอบ · Type your answer");
                x.CloseComponent();
            }
            x.OpenComponent<ShadcnQuestionnaireError>(12);
            x.CloseComponent();
        }));
        b.CloseComponent();
    }

    private static void AddChoice(RenderTreeBuilder b, int s, string value, string label, string description)
    {
        b.OpenComponent<ShadcnQuestionnaireChoice>(s);
        b.AddAttribute(s + 1, nameof(ShadcnQuestionnaireChoice.Value), value);
        b.AddAttribute(s + 2, nameof(ShadcnQuestionnaireChoice.ChildContent), (RenderFragment)(choice =>
        {
            choice.OpenComponent<ShadcnQuestionnaireChoiceDescription>(0);
            choice.AddAttribute(1, nameof(ShadcnQuestionnaireChoiceDescription.ChildContent), (RenderFragment)(content =>
            {
                content.OpenElement(0, "span");
                content.AddAttribute(1, "class", "showcase-questionnaire-choice-title");
                content.AddContent(2, label);
                content.CloseElement();
                content.OpenElement(3, "span");
                content.AddAttribute(4, "class", "showcase-questionnaire-choice-detail");
                content.AddContent(5, description);
                content.CloseElement();
            }));
            choice.CloseComponent();
        }));
        b.CloseComponent();
    }

    private static void AddScrollerMessage(RenderTreeBuilder b, int s, string id, ShadcnLogicalAlign align, AvatarProfile? avatar, string author, string text, bool anchor)
    {
        b.OpenComponent<ShadcnMessageScrollerItem>(s);
        b.AddAttribute(s + 1, nameof(ShadcnMessageScrollerItem.MessageId), id);
        b.AddAttribute(s + 2, nameof(ShadcnMessageScrollerItem.ScrollAnchor), anchor);
        b.AddAttribute(s + 3, nameof(ShadcnMessageScrollerItem.ChildContent), (RenderFragment)(item =>
            AddMessage(item, 0, align, avatar, false, author, text)));
        b.CloseComponent();
    }

    private static string AttachmentDescription(ShadcnAttachmentState state, string description, double? progress) => state switch
    {
        ShadcnAttachmentState.Uploading when progress.HasValue => $"Uploading · {progress.Value:0}% · {description}",
        ShadcnAttachmentState.Processing => $"Processing · {description}",
        ShadcnAttachmentState.Error => "Upload failed",
        _ => description
    };

    private static RenderFragment MarkerIconContent(bool streaming, string fallback) => builder =>
    {
        if (streaming)
        {
            builder.AddMarkupContent(0, "<span class=\"showcase-marker-loader shadcn-marker-loader\" aria-hidden=\"true\"></span>");
        }
        else
        {
            builder.AddContent(1, fallback);
        }
    };

    private const string MarkerRazorSource = """
@using Maliev.ShadcnBlazor.Components.Conversation

<div class="showcase-marker-thread showcase-bubble-thread">
    <ShadcnMessage Align="ShadcnLogicalAlign.Start">
        <ShadcnMessageAvatar><img src="images/avatars/assistant-thai.png" alt="MALIEV Assistant" /></ShadcnMessageAvatar>
        <ShadcnMessageContent>
            <ShadcnMessageHeader>MALIEV Assistant</ShadcnMessageHeader>
            <ShadcnBubble Align="ShadcnLogicalAlign.Start" Variant="ShadcnBubbleVariant.Ghost">
                <ShadcnBubbleContent dir="auto">Machining is complete. Bore 4 is ready for final inspection.</ShadcnBubbleContent>
            </ShadcnBubble>
        </ShadcnMessageContent>
    </ShadcnMessage>

    <ShadcnMessage Align="ShadcnLogicalAlign.End">
        <ShadcnMessageAvatar><img src="images/avatars/operator-thai.png" alt="Narin S." /></ShadcnMessageAvatar>
        <ShadcnMessageContent>
            <ShadcnMessageHeader>Narin S.</ShadcnMessageHeader>
            <ShadcnBubble Align="ShadcnLogicalAlign.End" Variant="ShadcnBubbleVariant.Default">
                <ShadcnBubbleContent dir="auto">Probe result received. I am uploading the signed report now.</ShadcnBubbleContent>
            </ShadcnBubble>
        </ShadcnMessageContent>
    </ShadcnMessage>

    <ShadcnMessage Align="ShadcnLogicalAlign.Start">
        <ShadcnMessageAvatar><img src="images/avatars/reviewer-thai.png" alt="Kanda T." /></ShadcnMessageAvatar>
        <ShadcnMessageContent>
            <ShadcnMessageHeader>Kanda T.</ShadcnMessageHeader>
            <ShadcnBubble Align="ShadcnLogicalAlign.Start" Variant="ShadcnBubbleVariant.Ghost">
                <ShadcnBubbleContent dir="auto">Thanks. I will hold dispatch until Quality signs off.</ShadcnBubbleContent>
            </ShadcnBubble>
        </ShadcnMessageContent>
    </ShadcnMessage>

    <ShadcnMarker Variant="ShadcnMarkerVariant.Default">
        <ShadcnMarkerIcon>✓</ShadcnMarkerIcon>
        <ShadcnMarkerContent dir="auto">Four inspection files verified</ShadcnMarkerContent>
    </ShadcnMarker>

    <ShadcnMarker Variant="ShadcnMarkerVariant.Separator">
        <ShadcnMarkerIcon>•</ShadcnMarkerIcon>
        <ShadcnMarkerContent dir="auto">14:32 · WO-2418</ShadcnMarkerContent>
    </ShadcnMarker>

    <ShadcnMarker Live="true" Variant="ShadcnMarkerVariant.Border">
        <ShadcnMarkerIcon>
            <span class="showcase-marker-loader shadcn-marker-loader" aria-hidden="true"></span>
        </ShadcnMarkerIcon>
        <ShadcnMarkerContent dir="auto" Streaming="true">Preparing quality handoff</ShadcnMarkerContent>
    </ShadcnMarker>
</div>
""";

    private const string BubbleRazorSource = """
@using Maliev.ShadcnBlazor.Components.Conversation

<ShadcnBubbleGroup data-reveal="true">
    <ShadcnBubble Align="ShadcnLogicalAlign.End" Variant="@sentVariant">
        <ShadcnBubbleContent>Hey there! what's up?</ShadcnBubbleContent>
    </ShadcnBubble>

    <ShadcnBubble Align="ShadcnLogicalAlign.Start" Variant="@receivedVariant">
        <ShadcnBubbleContent>Hey! Want to see chat bubbles?</ShadcnBubbleContent>
        <ShadcnBubbleReactions Side="ShadcnReactionSide.Bottom" Align="ShadcnLogicalAlign.Start" AccessibleName="Reactions for the message">
            <ShadcnBubbleReaction Fallback="👍"
                                  AccessibleName="Thumbs up reaction"
                                  Count="@thumbsUpCount"
                                  Pressed="@thumbsUpPressed"
                                  PressedChanged="SetThumbsUp" />
        </ShadcnBubbleReactions>
    </ShadcnBubble>

    <ShadcnBubble Align="ShadcnLogicalAlign.Start" Variant="@receivedVariant">
        <ShadcnBubbleContent>I can group messages, switch sides, and keep the whole thread easy to scan.</ShadcnBubbleContent>
    </ShadcnBubble>

    <ShadcnBubble Align="ShadcnLogicalAlign.End" Variant="@sentVariant">
        <ShadcnBubbleContent Href="/docs/components/bubble">Sure. Hit me with your best demo.</ShadcnBubbleContent>
    </ShadcnBubble>

    <ShadcnBubble Align="ShadcnLogicalAlign.Start" Variant="@receivedVariant">
        <ShadcnBubbleContent>Yes. You are reading a demo that is demoing itself. Very meta. Very on-brand.</ShadcnBubbleContent>
        <ShadcnBubbleReactions Side="ShadcnReactionSide.Bottom" Align="ShadcnLogicalAlign.Start" AccessibleName="Reactions for the message">
            <ShadcnBubbleReaction Fallback="❤️"
                                  AccessibleName="Heart reaction"
                                  Count="@heartCount"
                                  Pressed="@heartPressed"
                                  PressedChanged="SetHeart" />
            <ShadcnBubbleReaction Fallback="😂"
                                  AccessibleName="Laughing reaction"
                                  Count="@laughCount"
                                  Pressed="@laughPressed"
                                  PressedChanged="SetLaugh" />
            <ShadcnBubbleReactionOverflow Count="2">
                <ShadcnBubbleReaction Fallback="🔥"
                                      AccessibleName="Fire reaction"
                                      Count="@fireCount"
                                      Pressed="@firePressed"
                                      PressedChanged="SetFire" />
                <ShadcnBubbleReaction Fallback="👀"
                                      AccessibleName="Eyes reaction"
                                      Count="@eyesCount"
                                      Pressed="@eyesPressed"
                                      PressedChanged="SetEyes" />
            </ShadcnBubbleReactionOverflow>
        </ShadcnBubbleReactions>
    </ShadcnBubble>
</ShadcnBubbleGroup>

@code {
    private ShadcnBubbleVariant sentVariant = ShadcnBubbleVariant.Default;
    private ShadcnBubbleVariant receivedVariant = ShadcnBubbleVariant.Ghost;
    private int thumbsUpCount = 1;
    private int heartCount = 2;
    private int laughCount = 1;
    private int fireCount = 1;
    private int eyesCount = 1;
    private bool thumbsUpPressed;
    private bool heartPressed;
    private bool laughPressed;
    private bool firePressed;
    private bool eyesPressed;

    private void SetThumbsUp(bool pressed) => SetReaction(ref thumbsUpPressed, ref thumbsUpCount, pressed);
    private void SetHeart(bool pressed) => SetReaction(ref heartPressed, ref heartCount, pressed);
    private void SetLaugh(bool pressed) => SetReaction(ref laughPressed, ref laughCount, pressed);
    private void SetFire(bool pressed) => SetReaction(ref firePressed, ref fireCount, pressed);
    private void SetEyes(bool pressed) => SetReaction(ref eyesPressed, ref eyesCount, pressed);

    private static void SetReaction(ref bool current, ref int count, bool pressed)
    {
        if (current == pressed)
            return;

        current = pressed;
        count += pressed ? 1 : -1;
    }
}
""";

    private static void AddAvatar(RenderTreeBuilder b, int sequence, AvatarProfile profile)
    {
        b.OpenComponent<ShadcnMessageAvatar>(sequence);
        b.AddAttribute(sequence + 1, nameof(ShadcnMessageAvatar.ChildContent), (RenderFragment)(avatar =>
        {
            if (string.IsNullOrWhiteSpace(profile.Source))
            {
                avatar.OpenElement(0, "span");
                avatar.AddAttribute(1, "class", "showcase-message-avatar-placeholder");
                avatar.AddAttribute(2, "role", "img");
                avatar.AddAttribute(3, "aria-label", profile.Alt);
                avatar.AddAttribute(4, "data-avatar", profile.Id);
                avatar.AddContent(5, "ผ");
                avatar.CloseElement();
            }
            else
            {
                avatar.OpenElement(0, "img");
                avatar.AddAttribute(1, "class", "showcase-message-avatar-image");
                avatar.AddAttribute(2, "src", profile.Source);
                avatar.AddAttribute(3, "alt", profile.Alt);
                avatar.AddAttribute(4, "data-avatar", profile.Id);
                avatar.CloseElement();
            }
        }));
        b.CloseComponent();
    }
    private static RenderFragment UploadSpinner() => b =>
    {
        b.OpenComponent<ShadcnSpinner>(0);
        b.AddAttribute(1, nameof(ShadcnSpinner.Label), (string?)null);
        b.AddAttribute(2, nameof(ShadcnSpinner.SpinnerRole), ShadcnSpinnerRole.None);
        b.AddAttribute(3, nameof(ShadcnSpinner.Size), "1.25rem");
        b.AddAttribute(4, nameof(ShadcnSpinner.Class), "showcase-attachment-spinner");
        b.CloseComponent();
    };
    private static string ReplyIconMarkup() => "<svg class=\"shadcn-message-reply-icon\" viewBox=\"0 0 24 24\" aria-hidden=\"true\" focusable=\"false\"><path d=\"m9 7-5 5 5 5\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/><path d=\"M4 12h10a5 5 0 0 1 5 5v1\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\"/></svg>";
    private static ComponentExampleDefinition Example(string slug, string title, RenderFragment preview, IReadOnlyList<ComponentParameterControl> controls, IReadOnlyList<string> tags, string? razorSource = null) => new($"{slug}-primary", title, "Live package component with caller-owned localized state.", razorSource ?? $"<Shadcn{string.Concat(slug.Split('-').Select(w => char.ToUpperInvariant(w[0]) + w[1..]))} />", preview, controls, tags);

    private static string QuestionnaireRazorSource(bool includeNotes, string requestedStart)
    {
        var start = includeNotes && requestedStart == "notes" ? "notes" : "scope";
        var notesDefinition = includeNotes ? ",\n    new(\"notes\", AllowsFreeform: true)" : string.Empty;
        var notesMarkup = includeNotes ? """

    <ShadcnQuestionnaireItem Name="notes">
        <ShadcnQuestionnaireTitle>รายละเอียดเพิ่มเติม · Additional details</ShadcnQuestionnaireTitle>
        <ShadcnQuestionnaireDescription>อธิบายสิ่งที่ต้องการให้ทีมตรวจสอบ · Describe what the team should review</ShadcnQuestionnaireDescription>
        <ShadcnQuestionnaireInput AccessibleName="รายละเอียดเพิ่มเติม · Additional details" placeholder="พิมพ์คำตอบของคุณ · Type your answer" />
        <ShadcnQuestionnaireError />
    </ShadcnQuestionnaireItem>
""" : string.Empty;

        return $$"""
@using Maliev.ShadcnBlazor.Components.Conversation

<ShadcnQuestionnaire Items="Items" DefaultItem="{{start}}" AccessibleName="ขอบเขตงาน · Work scope" Shortcuts="ShadcnQuestionnaireShortcutMode.Numbers">
    <ShadcnQuestionnaireProgress AccessibleName="ความคืบหน้า · Progress" />
    <ShadcnQuestionnaireItem Name="scope">
        <ShadcnQuestionnaireTitle>เลือกประเภทการตรวจ · Choose a review type</ShadcnQuestionnaireTitle>
        <ShadcnQuestionnaireDescription>เลือก workflow ที่ต้องการสาธิต · Choose the workflow to demonstrate</ShadcnQuestionnaireDescription>
        <ShadcnQuestionnaireChoices>
            <ShadcnQuestionnaireChoice Value="component">Component</ShadcnQuestionnaireChoice>
            <ShadcnQuestionnaireChoice Value="feature">Feature</ShadcnQuestionnaireChoice>
            <ShadcnQuestionnaireChoice Value="other">อื่น ๆ · Other</ShadcnQuestionnaireChoice>
        </ShadcnQuestionnaireChoices>
        <ShadcnQuestionnaireInput AccessibleName="ระบุประเภทงาน · Describe the work" placeholder="พิมพ์คำตอบ · Type your answer" />
        <ShadcnQuestionnaireError />
    </ShadcnQuestionnaireItem>{{notesMarkup}}
    <ShadcnQuestionnaireActions>
        <ShadcnQuestionnairePrevious>ก่อนหน้า</ShadcnQuestionnairePrevious>
        <ShadcnQuestionnaireSkip>ข้าม</ShadcnQuestionnaireSkip>
        <ShadcnQuestionnaireNext>ถัดไป</ShadcnQuestionnaireNext>
        <ShadcnQuestionnaireSubmit>ส่งคำตอบ</ShadcnQuestionnaireSubmit>
    </ShadcnQuestionnaireActions>
</ShadcnQuestionnaire>

@code {
    private static readonly ShadcnQuestionnaireItemDefinition[] Items =
    [
        new("scope", Required: true, AllowsFreeform: true,
            Choices: [new("component"), new("feature"), new("other", Custom: true)]){{notesDefinition}}
    ];
}
""";
    }

    private const string MessageScrollerRazorSource = """
@using Maliev.ShadcnBlazor.Components.Conversation

<ShadcnMessageScrollerProvider AutoScroll="true" DefaultScrollPosition="ShadcnMessageDefaultScrollPosition.End">
    <ShadcnMessageScroller Class="showcase-scroller-frame" Style="height:24rem">
        <div class="showcase-scroller-transcript">
            <ShadcnMessageScrollerViewport AccessibleName="Project conversation">
                <ShadcnMessageScrollerContent>
                <ShadcnMessageScrollerItem MessageId="user-1" ScrollAnchor="true">
                    <ShadcnMessage Align="ShadcnLogicalAlign.End">
                        <ShadcnMessageAvatar><img src="images/avatars/operator-thai.png" alt="Operator" /></ShadcnMessageAvatar>
                        <ShadcnMessageContent>
                            <ShadcnMessageBody>
                                <ShadcnMessageHeader>Operator</ShadcnMessageHeader>
                                <ShadcnBubble Align="ShadcnLogicalAlign.End" Variant="ShadcnBubbleVariant.Muted">
                                    <ShadcnBubbleContent>เริ่มตรวจสอบชิ้นงานแล้ว</ShadcnBubbleContent>
                                </ShadcnBubble>
                            </ShadcnMessageBody>
                        </ShadcnMessageContent>
                    </ShadcnMessage>
                </ShadcnMessageScrollerItem>
                <ShadcnMessageScrollerItem MessageId="assistant-1" ScrollAnchor="true">
                    <ShadcnMessage Align="ShadcnLogicalAlign.Start">
                        <ShadcnMessageAvatar><img src="images/avatars/assistant-thai.png" alt="Assistant" /></ShadcnMessageAvatar>
                        <ShadcnMessageContent>
                            <ShadcnMessageBody>
                                <ShadcnMessageHeader>Assistant</ShadcnMessageHeader>
                                <ShadcnBubble Align="ShadcnLogicalAlign.Start" Variant="ShadcnBubbleVariant.Default">
                                    <ShadcnBubbleContent>รับทราบครับ ผมพร้อมสรุปผลการตรวจสอบให้แล้ว</ShadcnBubbleContent>
                                </ShadcnBubble>
                            </ShadcnMessageBody>
                        </ShadcnMessageContent>
                    </ShadcnMessage>
                </ShadcnMessageScrollerItem>
                </ShadcnMessageScrollerContent>
            </ShadcnMessageScrollerViewport>
            <ShadcnMessageScrollerButton Direction="ShadcnMessageScrollDirection.End" AccessibleName="Jump to latest">ข้อความล่าสุด</ShadcnMessageScrollerButton>
        </div>
        <form class="showcase-scroller-composer" @onsubmit:preventDefault="true">
            <input @bind="message" aria-label="New message" />
            <button type="submit" aria-label="Send message">Send</button>
        </form>
    </ShadcnMessageScroller>
</ShadcnMessageScrollerProvider>
""";

    private static string MessageRazorSource(bool middleRowEnd, bool avatars, bool footerActions, bool alwaysShowActions)
    {
        var operatorAvatar = avatars ? """
        <ShadcnMessageAvatar><img src="images/avatars/operator-thai.png" alt="Operator" /></ShadcnMessageAvatar>
""" : string.Empty;
        var coordinatorAvatar = avatars ? """
        <ShadcnMessageAvatar>
            <ShadcnAvatar Size="ShadcnAvatarSize.Small"><ShadcnAvatarFallback>ม</ShadcnAvatarFallback></ShadcnAvatar>
        </ShadcnMessageAvatar>
""" : string.Empty;
        var assistantAvatar = avatars ? """
        <ShadcnMessageAvatar><img src="images/avatars/assistant-thai.png" alt="Assistant" /></ShadcnMessageAvatar>
""" : string.Empty;
        var operatorFooter = footerActions ? $$"""
            <ShadcnMessageFooter{{(alwaysShowActions ? " data-visibility=\"always\"" : string.Empty)}}>
                <ShadcnMessageActions>
                    <ShadcnMessageCopyAction Text="ตรวจสอบไฟล์แล้ว 3 รายการ" />
                    <ShadcnMessageReplyAction Quote="ตรวจสอบไฟล์แล้ว 3 รายการ" OnReply="ReplyTo" />
                </ShadcnMessageActions>
            </ShadcnMessageFooter>
""" : string.Empty;
        var assistantFooter = footerActions ? $$"""
            <ShadcnMessageFooter{{(alwaysShowActions ? " data-visibility=\"always\"" : string.Empty)}}>
                <ShadcnMessageActions>
                    <ShadcnMessageCopyAction Text="Sure. I’ll keep the thread easy to scan." />
                    <ShadcnMessageReplyAction Quote="Sure. I’ll keep the thread easy to scan." OnReply="ReplyTo" />
                </ShadcnMessageActions>
                <ShadcnMessageStatus>ส่งแล้ว · 10:42</ShadcnMessageStatus>
            </ShadcnMessageFooter>
""" : string.Empty;
        var replyComposition = footerActions ? """

    @if (!string.IsNullOrWhiteSpace(replyText))
    {
        <ShadcnMessageReplyQuote Quote="@replyText" OnDismiss="ClearReply" />
    }
""" : string.Empty;
        var replyState = footerActions ? """

@code {
    private string? replyText;
    private void ReplyTo(string quote) => replyText = quote;
    private void ClearReply() => replyText = null;
}
""" : string.Empty;

        return $$"""
@using Maliev.ShadcnBlazor.Components.Conversation
@using Maliev.ShadcnBlazor.Components.Content

<ShadcnMessageGroup Class="message-thread">
    <ShadcnMessage Align="ShadcnLogicalAlign.Start">
{{operatorAvatar}}
        <ShadcnMessageContent>
            <ShadcnMessageBody>
                <ShadcnMessageHeader>Operator</ShadcnMessageHeader>
                <ShadcnBubble Align="ShadcnLogicalAlign.Start" Variant="ShadcnBubbleVariant.Muted">
                    <ShadcnBubbleContent dir="auto">ตรวจสอบไฟล์แล้ว 3 รายการ</ShadcnBubbleContent>
                </ShadcnBubble>
            </ShadcnMessageBody>
{{operatorFooter}}
        </ShadcnMessageContent>
    </ShadcnMessage>

    <ShadcnMessage Align="ShadcnLogicalAlign.{{(middleRowEnd ? "End" : "Start")}}">
{{coordinatorAvatar}}
        <ShadcnMessageContent>
            <ShadcnMessageBody>
                <ShadcnMessageHeader>ผู้ประสานงาน</ShadcnMessageHeader>
                <ShadcnBubble Align="ShadcnLogicalAlign.{{(middleRowEnd ? "End" : "Start")}}" Variant="ShadcnBubbleVariant.{{(middleRowEnd ? "Default" : "Muted")}}">
                    <ShadcnBubbleContent dir="auto">พร้อมส่งแบบให้ตรวจ</ShadcnBubbleContent>
                </ShadcnBubble>
            </ShadcnMessageBody>
        </ShadcnMessageContent>
    </ShadcnMessage>

    <ShadcnMessage Align="ShadcnLogicalAlign.End">
{{assistantAvatar}}
        <ShadcnMessageContent>
            <ShadcnMessageBody>
                <ShadcnMessageHeader>Assistant</ShadcnMessageHeader>
                <ShadcnBubble Align="ShadcnLogicalAlign.End" Variant="ShadcnBubbleVariant.Default">
                    <ShadcnBubbleContent dir="auto">Sure. I’ll keep the thread easy to scan.</ShadcnBubbleContent>
                </ShadcnBubble>
            </ShadcnMessageBody>
{{assistantFooter}}
        </ShadcnMessageContent>
    </ShadcnMessage>

{{replyComposition}}
</ShadcnMessageGroup>
{{replyState}}
""";
    }

    private const string AttachmentRazorSource = """
@using Maliev.ShadcnBlazor.Components.Feedback

<ShadcnAttachmentGroup AccessibleName="Uploaded files">
    <div class="attachment-gallery">
        <ShadcnAttachment State="ShadcnAttachmentState.Done" Orientation="ShadcnAttachmentOrientation.Vertical" Title="workspace-plan.png">
            <ShadcnAttachmentMedia Variant="ShadcnAttachmentMediaVariant.Image" ImageAlt="Preview of workspace-plan.png">
                <img src="images/attachments/workspace-plan.png" alt="" />
            </ShadcnAttachmentMedia>
            <ShadcnAttachmentContent>
                <ShadcnAttachmentTitle>workspace-plan.png</ShadcnAttachmentTitle>
                <ShadcnAttachmentDescription>PNG · 2.0 MB</ShadcnAttachmentDescription>
            </ShadcnAttachmentContent>
        </ShadcnAttachment>
        <ShadcnAttachment State="ShadcnAttachmentState.Done" Orientation="ShadcnAttachmentOrientation.Vertical" Title="desk-reference.png">
            <ShadcnAttachmentMedia Variant="ShadcnAttachmentMediaVariant.Image" ImageAlt="Preview of desk-reference.png">
                <img src="images/attachments/desk-reference.png" alt="" />
            </ShadcnAttachmentMedia>
            <ShadcnAttachmentContent>
                <ShadcnAttachmentTitle>desk-reference.png</ShadcnAttachmentTitle>
                <ShadcnAttachmentDescription>PNG · 2.0 MB</ShadcnAttachmentDescription>
            </ShadcnAttachmentContent>
        </ShadcnAttachment>
        <ShadcnAttachment State="ShadcnAttachmentState.Done" Orientation="ShadcnAttachmentOrientation.Vertical" Title="office-reference.png">
            <ShadcnAttachmentMedia Variant="ShadcnAttachmentMediaVariant.Image" ImageAlt="Preview of office-reference.png">
                <img src="images/attachments/office-reference.png" alt="" />
            </ShadcnAttachmentMedia>
            <ShadcnAttachmentContent>
                <ShadcnAttachmentTitle>office-reference.png</ShadcnAttachmentTitle>
                <ShadcnAttachmentDescription>PNG · 1.8 MB</ShadcnAttachmentDescription>
            </ShadcnAttachmentContent>
        </ShadcnAttachment>
    </div>

    <ShadcnAttachment State="ShadcnAttachmentState.Uploading" Progress="64" Title="sales-dashboard.pdf">
        <ShadcnAttachmentMedia>
            <ShadcnSpinner Label="Uploading" SpinnerRole="ShadcnSpinnerRole.None" Size="1.25rem" />
        </ShadcnAttachmentMedia>
        <ShadcnAttachmentContent>
            <ShadcnAttachmentTitle>sales-dashboard.pdf</ShadcnAttachmentTitle>
            <ShadcnAttachmentDescription>Uploading · 64% · PDF · 2.4 MB</ShadcnAttachmentDescription>
        </ShadcnAttachmentContent>
        <ShadcnAttachmentActions>
            <ShadcnAttachmentAction Action="ShadcnAttachmentActionKind.Remove" AccessibleName="Remove sales-dashboard.pdf">
                <span aria-hidden="true">×</span>
            </ShadcnAttachmentAction>
        </ShadcnAttachmentActions>
    </ShadcnAttachment>

    <ShadcnAttachment State="ShadcnAttachmentState.Done" Title="message-renderer.tsx">
        <ShadcnAttachmentMedia>
            <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false"><path d="M6 2.75h8l4 4v14.5H6z" fill="none" stroke="currentColor" stroke-width="1.6" /><path d="M14 2.75v4h4M9 12h6M9 16h4" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" /></svg>
        </ShadcnAttachmentMedia>
        <ShadcnAttachmentContent>
            <ShadcnAttachmentTitle>message-renderer.tsx</ShadcnAttachmentTitle>
            <ShadcnAttachmentDescription>TypeScript · 12 KB</ShadcnAttachmentDescription>
        </ShadcnAttachmentContent>
        <ShadcnAttachmentActions>
            <ShadcnAttachmentAction Action="ShadcnAttachmentActionKind.Remove" AccessibleName="Remove message-renderer.tsx">
                <span aria-hidden="true">×</span>
            </ShadcnAttachmentAction>
        </ShadcnAttachmentActions>
    </ShadcnAttachment>
</ShadcnAttachmentGroup>
""";
    private static ComponentParameterControl Toggle(string id, string label, Action<bool> apply, bool initial = false) => new(id, label, ComponentParameterControlKind.Toggle, initial.ToString(), [], v => apply(bool.Parse(v)));
    private static ComponentParameterControl Select(string id, string label, string initial, IReadOnlyList<string> options, Action<string> apply) => new(id, label, ComponentParameterControlKind.Select, initial, options, apply);
    private static RenderFragment Text(string value) => b => b.AddContent(0, value);
    private static void AddText<T>(RenderTreeBuilder b, int s, string text) where T : IComponent { b.OpenComponent<T>(s); b.AddAttribute(s + 1, "ChildContent", Text(text)); b.CloseComponent(); }
}
