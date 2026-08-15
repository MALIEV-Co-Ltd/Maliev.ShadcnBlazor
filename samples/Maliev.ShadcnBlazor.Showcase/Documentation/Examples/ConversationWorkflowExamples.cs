using Maliev.ShadcnBlazor.Components.Conversation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

internal static class ConversationWorkflowExamples
{
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
        var state = ShadcnAttachmentState.Uploading;
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
                AddImageAttachment(gallery, 0, "workspace-plan.svg", "SVG · 820 KB", "workspace", image);
                AddImageAttachment(gallery, 10, "desk-reference.jpg", "JPG · 1.1 MB", "desk", image);
                AddImageAttachment(gallery, 20, "office-reference.jpg", "JPG · 940 KB", "office", image);
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
                Select("attachment-state", "State", "Uploading", ["Idle", "Uploading", "Processing", "Error", "Done"], v => state = Enum.Parse<ShadcnAttachmentState>(v)),
                Toggle("attachment-vertical", "Vertical", v => vertical = v),
                Toggle("attachment-image", "Image media", v => image = v, true)
            ],
            ["gallery", "image", "file", "uploading", "processing", "error", "done", "progress", "actions", "group", "rtl"]);
    }

    private static ComponentExampleDefinition Bubble()
    {
        var variant = ShadcnBubbleVariant.Secondary;
        var end = false;
        var top = false;

        RenderFragment preview = b =>
        {
            b.OpenComponent<ShadcnBubbleGroup>(0);
            b.AddAttribute(1, nameof(ShadcnBubbleGroup.Class), "showcase-bubble-thread");
            b.AddAttribute(2, nameof(ShadcnBubbleGroup.ChildContent), (RenderFragment)(thread =>
            {
                AddBubble(thread, 0, ShadcnBubbleVariant.Default, ShadcnLogicalAlign.End, "Hey there! what's up?", false, null, top);
                AddBubble(thread, 10, variant, end ? ShadcnLogicalAlign.End : ShadcnLogicalAlign.Start, "Hey! Want to see chat bubbles?", false, "👍", top);
                AddBubble(thread, 20, ShadcnBubbleVariant.Muted, ShadcnLogicalAlign.Start, "Yes. You are reading a demo that is demoing itself. Very meta. Very on-brand.", false, null, top);
                AddBubble(thread, 30, ShadcnBubbleVariant.Default, ShadcnLogicalAlign.End, "Sure. Hit me with your best demo.", true, "👍 🔥 👀 +2", top);
            }));
            b.CloseComponent();
        };

        return Example(
            "bubble",
            "Conversation bubble",
            preview,
            [
                Select("bubble-variant", "Incoming variant", "Secondary", Enum.GetNames<ShadcnBubbleVariant>(), v => variant = Enum.Parse<ShadcnBubbleVariant>(v)),
                Toggle("bubble-end", "Align incoming end", v => end = v),
                Toggle("bubble-reactions-top", "Reactions top", v => top = v)
            ],
            ["thread", "variants", "alignment", "reactions", "button", "link", "collapsible", "rtl"]);
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
        b.AddAttribute(sequence + 4, nameof(ShadcnAttachment.ErrorReason), state == ShadcnAttachmentState.Error ? "Upload failed" : null);
        b.AddAttribute(sequence + 5, nameof(ShadcnAttachment.Class), $"showcase-attachment-file{(vertical ? " showcase-attachment-file--vertical" : string.Empty)}");
        b.AddAttribute(sequence + 6, nameof(ShadcnAttachment.ChildContent), (RenderFragment)(content =>
        {
            content.OpenComponent<ShadcnAttachmentMedia>(0);
            content.AddAttribute(1, nameof(ShadcnAttachmentMedia.Class), "showcase-attachment-file-icon");
            content.AddAttribute(2, nameof(ShadcnAttachmentMedia.ChildContent), FileIcon());
            content.CloseComponent();
            content.OpenComponent<ShadcnAttachmentContent>(3);
            content.AddAttribute(4, nameof(ShadcnAttachmentContent.ChildContent), (RenderFragment)(meta =>
            {
                AddText<ShadcnAttachmentTitle>(meta, 0, title);
                AddText<ShadcnAttachmentDescription>(meta, 3, state == ShadcnAttachmentState.Error ? "Upload failed" : description);
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

    private static void AddBubble(RenderTreeBuilder b, int sequence, ShadcnBubbleVariant variant, ShadcnLogicalAlign align, string text, bool interactive, string? reaction, bool reactionTop)
    {
        b.OpenComponent<ShadcnBubble>(sequence);
        b.AddAttribute(sequence + 1, nameof(ShadcnBubble.Variant), variant);
        b.AddAttribute(sequence + 2, nameof(ShadcnBubble.Align), align);
        b.AddAttribute(sequence + 3, nameof(ShadcnBubble.ChildContent), (RenderFragment)(content =>
        {
            content.OpenComponent<ShadcnBubbleContent>(0);
            if (interactive)
                content.AddAttribute(1, nameof(ShadcnBubbleContent.OnActivate), EventCallback.Factory.Create(new object(), () => { }));
            content.AddAttribute(2, nameof(ShadcnBubbleContent.ChildContent), Text(text));
            content.CloseComponent();
            if (reaction is not null)
            {
                content.OpenComponent<ShadcnBubbleReactions>(3);
                content.AddAttribute(4, nameof(ShadcnBubbleReactions.Side), reactionTop ? ShadcnReactionSide.Top : ShadcnReactionSide.Bottom);
                content.AddAttribute(5, nameof(ShadcnBubbleReactions.Align), align == ShadcnLogicalAlign.End ? ShadcnLogicalAlign.End : ShadcnLogicalAlign.Start);
                content.AddAttribute(6, nameof(ShadcnBubbleReactions.AccessibleName), $"Reactions for {text}");
                content.AddAttribute(7, nameof(ShadcnBubbleReactions.ChildContent), ReactionMarkup(reaction));
                content.CloseComponent();
            }
        }));
        b.CloseComponent();
    }

    private static RenderFragment Thumbnail(string artwork) => b =>
        b.AddMarkupContent(0, artwork switch
        {
            "workspace" => "<svg class=\"showcase-attachment-artwork\" viewBox=\"0 0 160 120\" aria-hidden=\"true\"><rect width=\"160\" height=\"120\" fill=\"#d9dde2\"/><path d=\"M0 80 55 25l30 30 24-24 51 54H0Z\" fill=\"#8f9da7\"/><path d=\"M12 0v120M38 0v120M64 0v120M90 0v120M116 0v120M142 0v120\" stroke=\"#65737c\" stroke-width=\"3\" opacity=\".6\"/></svg>",
            "desk" => "<svg class=\"showcase-attachment-artwork\" viewBox=\"0 0 160 120\" aria-hidden=\"true\"><rect width=\"160\" height=\"120\" fill=\"#dbe8e4\"/><rect x=\"14\" y=\"12\" width=\"8\" height=\"108\" fill=\"#6f8d84\"/><rect x=\"118\" y=\"0\" width=\"5\" height=\"120\" fill=\"#90aaa4\"/><path d=\"M32 92h98L92 60 58 74 32 92Z\" fill=\"#a78d67\"/><rect x=\"74\" y=\"38\" width=\"34\" height=\"21\" rx=\"2\" fill=\"#34464d\"/></svg>",
            _ => "<svg class=\"showcase-attachment-artwork\" viewBox=\"0 0 160 120\" aria-hidden=\"true\"><rect width=\"160\" height=\"120\" fill=\"#d8d2c7\"/><path d=\"M0 96 52 40l36 29 28-42 44 69H0Z\" fill=\"#8a7967\"/><path d=\"M0 24h160M0 48h160M0 72h160\" stroke=\"#665947\" stroke-width=\"5\" opacity=\".45\"/><circle cx=\"118\" cy=\"30\" r=\"12\" fill=\"#c3a35f\"/></svg>"
        });

    private static RenderFragment FileIcon() => b => b.AddMarkupContent(0, "<svg viewBox=\"0 0 24 24\" aria-hidden=\"true\" focusable=\"false\"><path d=\"M6 2.75h8l4 4v14.5H6z\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.6\"/><path d=\"M14 2.75v4h4M9 12h6M9 16h4\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.6\" stroke-linecap=\"round\"/></svg>");
    private static RenderFragment CloseIcon() => b => b.AddMarkupContent(0, "<svg viewBox=\"0 0 24 24\" aria-hidden=\"true\" focusable=\"false\"><path d=\"m7 7 10 10M17 7 7 17\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\"/></svg>");
    private static RenderFragment ReactionMarkup(string value) => b => b.AddMarkupContent(0, value == "👍"
        ? "<span class=\"showcase-bubble-reaction-set\"><svg class=\"showcase-bubble-reaction-icon\" viewBox=\"0 0 24 24\" aria-hidden=\"true\"><path d=\"M8.5 10.5v9h-3v-9zM10 10.5l3.8-7.1a1.5 1.5 0 0 1 2.8 1l-.8 4.1h4.1a2 2 0 0 1 2 2.4l-1.3 6.3a3 3 0 0 1-2.9 2.4H8.5\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.6\" stroke-linejoin=\"round\"/></svg></span>"
        : "<span class=\"showcase-bubble-reaction-set\"><svg class=\"showcase-bubble-reaction-icon\" viewBox=\"0 0 24 24\" aria-hidden=\"true\"><path d=\"M13.4 2.8c.5 3.2-1.4 4.1-2.4 5.7-.8 1.3-.7 2.8.2 3.7.1-1.8 1.3-2.6 2.3-3.4.9 1.5 3.4 2.7 3.4 6a5 5 0 1 1-9.8-1.4c.6-2.4 2.6-3.9 3.6-5.8.8-1.5 1.2-3.2.6-4.8 1.1.5 1.7 1.1 2.1 2.2Z\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.5\" stroke-linejoin=\"round\"/><path d=\"M18.1 5.3v3.4M16.4 7h3.4\" stroke=\"currentColor\" stroke-width=\"1.3\" stroke-linecap=\"round\"/></svg><svg class=\"showcase-bubble-reaction-icon\" viewBox=\"0 0 24 24\" aria-hidden=\"true\"><path d=\"M2.5 12s3.3-5 9.5-5 9.5 5 9.5 5-3.3 5-9.5 5-9.5-5-9.5-5Z\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.5\"/><circle cx=\"12\" cy=\"12\" r=\"2.2\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.5\"/></svg><span class=\"showcase-bubble-reaction-count\">+2</span></span>");

    private static ComponentExampleDefinition Marker()
    {
        var variant = ShadcnMarkerVariant.Default;
        var streaming = true;
        RenderFragment preview = b =>
        {
            b.OpenElement(0, "div");
            b.AddAttribute(1, "class", "showcase-marker-thread");
            AddMarker(b, 2, variant, false, "✓", "ตรวจสอบ 4 ไฟล์แล้ว");
            AddMarker(b, 12, ShadcnMarkerVariant.Separator, false, "•", "วันนี้");
            AddMarker(b, 22, ShadcnMarkerVariant.Border, streaming, "✦", streaming ? "กำลังประมวลผล" : "พร้อมส่งให้ผู้ตรวจ");
            b.CloseElement();
        };
        return Example("marker", "Conversation marker", preview, [Select("marker-variant", "Primary variant", "Default", Enum.GetNames<ShadcnMarkerVariant>(), v => variant = Enum.Parse<ShadcnMarkerVariant>(v)), Toggle("marker-streaming", "Streaming status", v => streaming = v, true)], ["status", "separator", "border", "icon", "streaming", "shimmer", "reduced-motion"]);
    }

    private static ComponentExampleDefinition Message()
    {
        var end = false;
        var avatar = true;
        var footer = true;
        RenderFragment preview = b =>
        {
            b.OpenComponent<ShadcnMessageGroup>(0);
            b.AddAttribute(1, nameof(ShadcnMessageGroup.Class), "showcase-message-thread");
            b.AddAttribute(2, nameof(ShadcnMessageGroup.ChildContent), (RenderFragment)(thread =>
            {
                AddMessage(thread, 0, ShadcnLogicalAlign.Start, avatar, footer, "นที", "วิศวกร MALIEV", "ตรวจสอบไฟล์แล้ว 3 รายการ");
                AddMessage(thread, 20, end ? ShadcnLogicalAlign.End : ShadcnLogicalAlign.Start, avatar, false, "มาลี", "ผู้ประสานงาน", "พร้อมส่งแบบให้ตรวจ");
                AddMessage(thread, 40, ShadcnLogicalAlign.End, avatar, footer, "M", "MALIEV Assistant", "Sure. I’ll keep the thread easy to scan.");
            }));
            b.CloseComponent();
        };
        return Example("message", "Message row", preview, [Toggle("message-end", "Align middle row end", v => end = v), Toggle("message-avatar", "Avatars", v => avatar = v, true), Toggle("message-footer", "Footer actions", v => footer = v, true)], ["group", "start", "end", "avatar", "header", "footer", "bubbles", "rtl"]);
    }

    private static void AddMarker(RenderTreeBuilder b, int sequence, ShadcnMarkerVariant variant, bool live, string icon, string text)
    {
        b.OpenComponent<ShadcnMarker>(sequence);
        b.AddAttribute(sequence + 1, nameof(ShadcnMarker.Variant), variant);
        b.AddAttribute(sequence + 2, nameof(ShadcnMarker.Live), live);
        b.AddAttribute(sequence + 3, nameof(ShadcnMarker.ChildContent), (RenderFragment)(content =>
        {
            AddText<ShadcnMarkerIcon>(content, 0, icon);
            content.OpenComponent<ShadcnMarkerContent>(3);
            content.AddAttribute(4, nameof(ShadcnMarkerContent.Streaming), live);
            content.AddAttribute(5, nameof(ShadcnMarkerContent.ChildContent), Text(text));
            content.CloseComponent();
        }));
        b.CloseComponent();
    }

    private static void AddMessage(RenderTreeBuilder b, int sequence, ShadcnLogicalAlign align, bool avatar, bool footer, string avatarText, string author, string message)
    {
        b.OpenComponent<ShadcnMessage>(sequence);
        b.AddAttribute(sequence + 1, nameof(ShadcnMessage.Align), align);
        b.AddAttribute(sequence + 2, nameof(ShadcnMessage.ChildContent), (RenderFragment)(row =>
        {
            if (avatar)
                AddText<ShadcnMessageAvatar>(row, 0, avatarText);
            row.OpenComponent<ShadcnMessageContent>(3);
            row.AddAttribute(4, nameof(ShadcnMessageContent.ChildContent), (RenderFragment)(content =>
            {
                AddText<ShadcnMessageHeader>(content, 0, author);
                content.OpenComponent<ShadcnBubble>(3);
                content.AddAttribute(4, nameof(ShadcnBubble.Variant), align == ShadcnLogicalAlign.End ? ShadcnBubbleVariant.Default : ShadcnBubbleVariant.Muted);
                content.AddAttribute(5, nameof(ShadcnBubble.Align), align);
                content.AddAttribute(6, nameof(ShadcnBubble.ChildContent), (RenderFragment)(bubble => AddText<ShadcnBubbleContent>(bubble, 0, message)));
                content.CloseComponent();
                if (footer)
                    AddText<ShadcnMessageFooter>(content, 8, align == ShadcnLogicalAlign.End ? "ส่งแล้ว · 10:42" : "อ่านแล้ว · 10:42");
            }));
            row.CloseComponent();
        }));
        b.CloseComponent();
    }

    private static ComponentExampleDefinition Scroller()
    {
        var auto = false; var extra = false; var position = ShadcnMessageDefaultScrollPosition.End;
        RenderFragment preview = b => { b.OpenComponent<ShadcnMessageScrollerProvider>(0); b.SetKey($"{auto}-{extra}-{position}"); b.AddAttribute(1, "AutoScroll", auto); b.AddAttribute(2, "DefaultScrollPosition", position); b.AddAttribute(3, "ChildContent", (RenderFragment)(p => { p.OpenComponent<ShadcnMessageScroller>(0); p.AddAttribute(1, "Style", "height:16rem"); p.AddAttribute(2, "data-preview-auto", auto ? "true" : "false"); p.AddAttribute(3, "data-preview-position", position.ToString().ToLowerInvariant()); p.AddAttribute(4, "ChildContent", (RenderFragment)(r => { r.OpenComponent<ShadcnMessageScrollerViewport>(0); r.AddAttribute(1, "AccessibleName", "บทสนทนา"); r.AddAttribute(2, "ChildContent", (RenderFragment)(v => { v.OpenComponent<ShadcnMessageScrollerContent>(0); v.AddAttribute(1, "ChildContent", (RenderFragment)(c => { AddScrollerItem(c, 0, "turn-1", "ข้อความแรก", true); if (extra) AddScrollerItem(c, 5, "turn-2", "ข้อความใหม่", true); })); v.CloseComponent(); })); r.CloseComponent(); r.OpenComponent<ShadcnMessageScrollerButton>(5); r.AddAttribute(6, "AccessibleName", "ไปข้อความล่าสุด"); r.CloseComponent(); })); p.CloseComponent(); })); b.CloseComponent(); };
        return Example("message-scroller", "Streaming transcript", preview, [Toggle("scroller-auto", "Auto follow", v => auto = v), Toggle("scroller-append", "Append unread turn", v => extra = v), Select("scroller-position", "Opening position", "End", Enum.GetNames<ShadcnMessageDefaultScrollPosition>(), v => position = Enum.Parse<ShadcnMessageDefaultScrollPosition>(v))], ["anchor", "auto-follow", "user-intent", "unread", "jump", "prepend", "visibility", "focus", "rtl"]);
    }

    private static ComponentExampleDefinition Questionnaire()
    {
        var branch = true; var start = "scope";
        RenderFragment preview = b => { var items = branch ? new[] { new ShadcnQuestionnaireItemDefinition("scope", Required: true, Choices: [new("component"), new("feature")]), new("notes", AllowsFreeform: true) } : [new ShadcnQuestionnaireItemDefinition("scope", Required: true, Choices: [new("component"), new("feature")])]; b.OpenComponent<ShadcnQuestionnaire>(0); b.SetKey($"{branch}-{start}"); b.AddAttribute(1, "Items", items); b.AddAttribute(2, "DefaultItem", start == "notes" && branch ? "notes" : "scope"); b.AddAttribute(3, "AccessibleName", "ขอบเขตงาน"); b.AddAttribute(4, "ChildContent", (RenderFragment)(x => { x.OpenComponent<ShadcnQuestionnaireProgress>(0); x.AddAttribute(1, "AccessibleName", "Progress"); x.CloseComponent(); AddQuestion(x, 3, "scope", "เลือกขอบเขต", false); if (branch) AddQuestion(x, 10, "notes", "รายละเอียด", true); x.OpenComponent<ShadcnQuestionnaireActions>(20); x.AddAttribute(21, "ChildContent", (RenderFragment)(a => { AddText<ShadcnQuestionnairePrevious>(a, 0, "ก่อนหน้า"); AddText<ShadcnQuestionnaireSkip>(a, 3, "ข้าม"); AddText<ShadcnQuestionnaireNext>(a, 6, "ถัดไป"); AddText<ShadcnQuestionnaireSubmit>(a, 9, "ส่ง"); })); x.CloseComponent(); })); b.CloseComponent(); };
        return Example("questionnaire", "Guided questionnaire", preview, [Toggle("questionnaire-branch", "Conditional notes", v => branch = v, true), Select("questionnaire-start", "Resume item", "scope", ["scope", "notes"], v => start = v)], ["single", "multiple", "freeform", "skipped", "required", "invalid", "controlled", "resume", "branching", "submit", "thai", "rtl"]);
    }

    private static void AddQuestion(RenderTreeBuilder b, int s, string name, string title, bool input) { b.OpenComponent<ShadcnQuestionnaireItem>(s); b.AddAttribute(s + 1, "Name", name); b.AddAttribute(s + 2, "ChildContent", (RenderFragment)(x => { AddText<ShadcnQuestionnaireTitle>(x, 0, title); if (input) { x.OpenComponent<ShadcnQuestionnaireInput>(3); x.AddAttribute(4, "AccessibleName", title); x.CloseComponent(); } else { x.OpenComponent<ShadcnQuestionnaireChoices>(3); x.AddAttribute(4, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnQuestionnaireChoice>(0); c.AddAttribute(1, "Value", "component"); c.AddAttribute(2, "ChildContent", Text("Component")); c.CloseComponent(); c.OpenComponent<ShadcnQuestionnaireChoice>(3); c.AddAttribute(4, "Value", "feature"); c.AddAttribute(5, "ChildContent", Text("Feature")); c.CloseComponent(); })); x.CloseComponent(); } x.OpenComponent<ShadcnQuestionnaireError>(8); x.CloseComponent(); })); b.CloseComponent(); }
    private static void AddScrollerItem(RenderTreeBuilder b, int s, string id, string text, bool anchor) { b.OpenComponent<ShadcnMessageScrollerItem>(s); b.AddAttribute(s + 1, "MessageId", id); b.AddAttribute(s + 2, "ScrollAnchor", anchor); b.AddAttribute(s + 3, "ChildContent", Text(text)); b.CloseComponent(); }
    private static ComponentExampleDefinition Example(string slug, string title, RenderFragment preview, IReadOnlyList<ComponentParameterControl> controls, IReadOnlyList<string> tags) => new($"{slug}-primary", title, "Live package component with caller-owned localized state.", $"<Shadcn{string.Concat(slug.Split('-').Select(w => char.ToUpperInvariant(w[0]) + w[1..]))} />", preview, controls, tags);
    private static ComponentParameterControl Toggle(string id, string label, Action<bool> apply, bool initial = false) => new(id, label, ComponentParameterControlKind.Toggle, initial.ToString(), [], v => apply(bool.Parse(v)));
    private static ComponentParameterControl Select(string id, string label, string initial, IReadOnlyList<string> options, Action<string> apply) => new(id, label, ComponentParameterControlKind.Select, initial, options, apply);
    private static RenderFragment Text(string value) => b => b.AddContent(0, value);
    private static void AddText<T>(RenderTreeBuilder b, int s, string text) where T : IComponent { b.OpenComponent<T>(s); b.AddAttribute(s + 1, "ChildContent", Text(text)); b.CloseComponent(); }
}
