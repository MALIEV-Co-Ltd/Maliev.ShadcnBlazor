using Maliev.ShadcnBlazor.Components.Conversation;
using Maliev.ShadcnBlazor.Components.Feedback;
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
                AddBubble(thread, 10, variant, end ? ShadcnLogicalAlign.End : ShadcnLogicalAlign.Start, "Hey! Want to see chat bubbles?", false, "👍", top, "incoming");
                AddBubble(thread, 20, variant, end ? ShadcnLogicalAlign.End : ShadcnLogicalAlign.Start, "I can group messages, switch sides, and keep the whole thread easy to scan.", false, null, top, "incoming");
                AddBubble(thread, 30, ShadcnBubbleVariant.Default, ShadcnLogicalAlign.End, "Sure. Hit me with your best demo.", true, null, top);
                AddBubble(thread, 40, variant, end ? ShadcnLogicalAlign.End : ShadcnLogicalAlign.Start, "Yes. You are reading a demo that is demoing itself. Very meta. Very on-brand.", false, "👍 🔥 👀 +2", top, "incoming");
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

    private static void AddBubble(RenderTreeBuilder b, int sequence, ShadcnBubbleVariant variant, ShadcnLogicalAlign align, string text, bool interactive, string? reaction, bool reactionTop, string? role = null)
    {
        b.OpenComponent<ShadcnBubble>(sequence);
        b.AddAttribute(sequence + 1, nameof(ShadcnBubble.Variant), variant);
        b.AddAttribute(sequence + 2, nameof(ShadcnBubble.Align), align);
        if (role is not null)
            b.AddAttribute(sequence + 3, "data-bubble-role", role);
        b.AddAttribute(sequence + 4, nameof(ShadcnBubble.ChildContent), (RenderFragment)(content =>
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
    private static RenderFragment ReactionMarkup(string value) => b => b.AddMarkupContent(0, value == "👍"
        ? "<span class=\"showcase-bubble-reaction-set\"><svg class=\"showcase-bubble-reaction-icon showcase-bubble-reaction-icon--like\" viewBox=\"0 0 24 24\" aria-hidden=\"true\"><path d=\"M8.5 10.5v9h-3v-9zM10 10.5l3.8-7.1a1.5 1.5 0 0 1 2.8 1l-.8 4.1h4.1a2 2 0 0 1 2 2.4l-1.3 6.3a3 3 0 0 1-2.9 2.4H8.5\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.6\" stroke-linejoin=\"round\"/></svg></span>"
        : "<span class=\"showcase-bubble-reaction-set\"><svg class=\"showcase-bubble-reaction-icon showcase-bubble-reaction-icon--fire\" viewBox=\"0 0 24 24\" aria-hidden=\"true\"><path d=\"M13.4 2.8c.5 3.2-1.4 4.1-2.4 5.7-.8 1.3-.7 2.8.2 3.7.1-1.8 1.3-2.6 2.3-3.4.9 1.5 3.4 2.7 3.4 6a5 5 0 1 1-9.8-1.4c.6-2.4 2.6-3.9 3.6-5.8.8-1.5 1.2-3.2.6-4.8 1.1.5 1.7 1.1 2.1 2.2Z\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.5\" stroke-linejoin=\"round\"/><path d=\"M18.1 5.3v3.4M16.4 7h3.4\" stroke=\"currentColor\" stroke-width=\"1.3\" stroke-linecap=\"round\"/></svg><svg class=\"showcase-bubble-reaction-icon showcase-bubble-reaction-icon--eyes\" viewBox=\"0 0 24 24\" aria-hidden=\"true\"><path d=\"M2.5 12s3.3-5 9.5-5 9.5 5 9.5 5-3.3 5-9.5 5-9.5-5-9.5-5Z\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.5\"/><circle cx=\"12\" cy=\"12\" r=\"2.2\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.5\"/></svg><span class=\"showcase-bubble-reaction-count\">+2</span></span>");

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
        return Example("marker", "Conversation marker", preview, [Select("marker-variant", "Primary variant", "Default", Enum.GetNames<ShadcnMarkerVariant>(), v => variant = Enum.Parse<ShadcnMarkerVariant>(v)), Toggle("marker-streaming", "Streaming status", v => streaming = v, true)], ["status", "separator", "border", "icon", "streaming", "shimmer", "reduced-motion"], MarkerRazorSource);
    }

    private static ComponentExampleDefinition Message()
    {
        var end = false;
        var avatar = true;
        var footer = true;
        var footerAlways = false;
        RenderFragment preview = b =>
        {
            b.OpenComponent<ShadcnMessageGroup>(0);
            b.AddAttribute(1, nameof(ShadcnMessageGroup.Class), $"showcase-message-thread{(footerAlways ? " showcase-message-thread--footer-always" : string.Empty)}");
            b.AddAttribute(2, nameof(ShadcnMessageGroup.ChildContent), (RenderFragment)(thread =>
            {
                AddMessage(thread, 0, ShadcnLogicalAlign.Start, avatar ? OperatorAvatar : null, footer, "วิศวกร MALIEV", "ตรวจสอบไฟล์แล้ว 3 รายการ", footerAlways);
                AddMessage(thread, 20, end ? ShadcnLogicalAlign.End : ShadcnLogicalAlign.Start, null, false, "ผู้ประสานงาน", "พร้อมส่งแบบให้ตรวจ");
                AddMessage(thread, 40, ShadcnLogicalAlign.End, avatar ? AssistantAvatar : null, footer, "MALIEV Assistant", "Sure. I’ll keep the thread easy to scan.", footerAlways);
            }));
            b.CloseComponent();
        };
        return Example("message", "Message row", preview, [Toggle("message-end", "Align middle row end", v => end = v), Toggle("message-avatar", "Avatars", v => avatar = v, true), Toggle("message-footer", "Footer actions", v => footer = v, true), Toggle("message-footer-always", "Always show actions", v => footerAlways = v)], ["group", "start", "end", "avatar", "header", "footer", "hover-actions", "bubbles", "rtl"], MessageRazorSource);
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
            content.CloseComponent();
        }));
        b.CloseComponent();
    }

    private static void AddMessage(RenderTreeBuilder b, int sequence, ShadcnLogicalAlign align, AvatarProfile? avatar, bool footer, string author, string message, bool footerAlways = false)
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
                content.AddAttribute(4, nameof(ShadcnBubble.Variant), align == ShadcnLogicalAlign.End ? ShadcnBubbleVariant.Default : ShadcnBubbleVariant.Muted);
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
        var auto = false; var extra = false; var position = ShadcnMessageDefaultScrollPosition.End;
        RenderFragment preview = b =>
        {
            b.OpenComponent<ShadcnMessageScrollerProvider>(0);
            b.SetKey($"{auto}-{extra}-{position}");
            b.AddAttribute(1, nameof(ShadcnMessageScrollerProvider.AutoScroll), auto);
            b.AddAttribute(2, nameof(ShadcnMessageScrollerProvider.DefaultScrollPosition), position);
            b.AddAttribute(3, nameof(ShadcnMessageScrollerProvider.ChildContent), (RenderFragment)(provider =>
            {
                provider.OpenComponent<ShadcnMessageScroller>(0);
                provider.AddAttribute(1, "class", "showcase-scroller-frame");
                provider.AddAttribute(2, "style", "height:24rem");
                provider.AddAttribute(3, "data-preview-auto", auto ? "true" : "false");
                provider.AddAttribute(4, "data-preview-position", position.ToString().ToLowerInvariant());
                provider.AddAttribute(5, nameof(ShadcnMessageScroller.ChildContent), (RenderFragment)(scroller =>
                {
                    scroller.OpenComponent<ShadcnMessageScrollerViewport>(0);
                    scroller.AddAttribute(1, nameof(ShadcnMessageScrollerViewport.AccessibleName), "บทสนทนาโครงการ");
                    scroller.AddAttribute(2, nameof(ShadcnMessageScrollerViewport.ChildContent), (RenderFragment)(viewport =>
                    {
                        viewport.OpenComponent<ShadcnMessageScrollerContent>(0);
                        viewport.AddAttribute(1, nameof(ShadcnMessageScrollerContent.ChildContent), (RenderFragment)(content =>
                        {
                            AddScrollerMessage(content, 0, "turn-1", ShadcnLogicalAlign.Start, OperatorAvatar, "วิศวกร MALIEV", "เริ่มตรวจสอบชิ้นงานแล้ว", true);
                            AddScrollerMessage(content, 5, "turn-2", ShadcnLogicalAlign.Start, null, "ผู้ประสานงาน", "พบไฟล์ CAD ครบ 3 รายการ", true);
                            AddScrollerMessage(content, 10, "turn-3", ShadcnLogicalAlign.End, AssistantAvatar, "MALIEV Assistant", "กำลังเตรียมใบเสนอราคา", true);
                            AddScrollerMessage(content, 15, "turn-4", ShadcnLogicalAlign.Start, CoordinatorAvatar, "ผู้ประสานงาน", "จะส่งให้ตรวจในอีกสักครู่", true);
                            AddScrollerMessage(content, 20, "turn-5", ShadcnLogicalAlign.End, null, "MALIEV Assistant", "รับทราบครับ", true);
                            if (extra)
                                AddScrollerMessage(content, 25, "turn-6", ShadcnLogicalAlign.Start, ReviewerAvatar, "ผู้ตรวจสอบคุณภาพ", "มีข้อความใหม่เข้ามา", true);
                        }));
                        viewport.CloseComponent();
                    }));
                    scroller.CloseComponent();
                    scroller.OpenComponent<ShadcnMessageScrollerButton>(5);
                    scroller.AddAttribute(6, nameof(ShadcnMessageScrollerButton.AccessibleName), "ไปข้อความล่าสุด");
                    scroller.AddAttribute(7, "ChildContent", Text("ข้อความล่าสุด"));
                    scroller.CloseComponent();
                    scroller.OpenElement(8, "form");
                    scroller.AddAttribute(9, "class", "showcase-scroller-composer");
                    scroller.AddAttribute(10, "aria-label", "ส่งข้อความ");
                    scroller.OpenElement(11, "input");
                    scroller.AddAttribute(12, "type", "text");
                    scroller.AddAttribute(13, "value", "เมื่อมีข้อความใหม่ ระบบจะจัดตำแหน่งรายการให้อ่านต่อได้โดยไม่รบกวนผู้ใช้");
                    scroller.AddAttribute(14, "aria-label", "ข้อความใหม่");
                    scroller.CloseElement();
                    scroller.OpenElement(15, "button");
                    scroller.AddAttribute(16, "type", "button");
                    scroller.AddAttribute(17, "aria-label", "ส่งข้อความ");
                    scroller.AddAttribute(18, "data-testid", "scroller-send");
                    scroller.AddMarkupContent(19, "<svg viewBox=\"0 0 24 24\" aria-hidden=\"true\" focusable=\"false\"><path d=\"m5 12 14-7-4 14-3.5-5.5Z\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linejoin=\"round\"/><path d=\"M11.5 13.5 19 5\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\"/></svg>");
                    scroller.CloseElement();
                    scroller.CloseElement();
                }));
                provider.CloseComponent();
            }));
            b.CloseComponent();
        };
        return Example("message-scroller", "Streaming transcript", preview, [Toggle("scroller-auto", "Auto follow", v => auto = v), Toggle("scroller-append", "Append unread turn", v => extra = v), Select("scroller-position", "Opening position", "End", Enum.GetNames<ShadcnMessageDefaultScrollPosition>(), v => position = Enum.Parse<ShadcnMessageDefaultScrollPosition>(v))], ["anchor", "auto-follow", "user-intent", "unread", "jump", "prepend", "visibility", "focus", "rtl"]);
    }

    private static ComponentExampleDefinition Questionnaire()
    {
        var branch = true; var start = "scope";
        RenderFragment preview = b =>
        {
            var items = branch
                ? new[] { new ShadcnQuestionnaireItemDefinition("scope", Required: true, Choices: [new("component"), new("feature")]), new ShadcnQuestionnaireItemDefinition("notes", AllowsFreeform: true) }
                : [new ShadcnQuestionnaireItemDefinition("scope", Required: true, Choices: [new("component"), new("feature")])];
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
        return Example("questionnaire", "Guided questionnaire", preview, [Toggle("questionnaire-branch", "Conditional notes", v => branch = v, true), Select("questionnaire-start", "Resume item", "scope", ["scope", "notes"], v => start = v)], ["single", "multiple", "freeform", "skipped", "required", "invalid", "controlled", "resume", "branching", "submit", "thai", "rtl"]);
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
                }));
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

<div>
    <ShadcnMarker Variant="ShadcnMarkerVariant.Default">
        <ShadcnMarkerIcon>✓</ShadcnMarkerIcon>
        <ShadcnMarkerContent>ตรวจสอบ 4 ไฟล์แล้ว</ShadcnMarkerContent>
    </ShadcnMarker>

    <ShadcnMarker Variant="ShadcnMarkerVariant.Separator">
        <ShadcnMarkerIcon>•</ShadcnMarkerIcon>
        <ShadcnMarkerContent>วันนี้</ShadcnMarkerContent>
    </ShadcnMarker>

    <ShadcnMarker Live="true" Variant="ShadcnMarkerVariant.Border">
        <ShadcnMarkerIcon>
            <span class="showcase-marker-loader shadcn-marker-loader" aria-hidden="true"></span>
        </ShadcnMarkerIcon>
        <ShadcnMarkerContent Streaming="true">กำลังประมวลผล</ShadcnMarkerContent>
    </ShadcnMarker>
</div>
""";

    private const string BubbleRazorSource = """
@using Maliev.ShadcnBlazor.Components.Conversation

<ShadcnBubbleGroup>
    <ShadcnBubble Align="ShadcnLogicalAlign.End" Variant="ShadcnBubbleVariant.Default">
        <ShadcnBubbleContent>Hey there! what's up?</ShadcnBubbleContent>
    </ShadcnBubble>

    <ShadcnBubble Align="ShadcnLogicalAlign.Start" Variant="ShadcnBubbleVariant.Tinted">
        <ShadcnBubbleContent>Hey! Want to see chat bubbles?</ShadcnBubbleContent>
        <ShadcnBubbleReactions Side="ShadcnReactionSide.Bottom" Align="ShadcnLogicalAlign.Start" AccessibleName="Reactions for the message">
            <span aria-hidden="true">👍</span>
        </ShadcnBubbleReactions>
    </ShadcnBubble>

    <ShadcnBubble Align="ShadcnLogicalAlign.Start" Variant="ShadcnBubbleVariant.Muted">
        <ShadcnBubbleContent>I can group messages, switch sides, and keep the whole thread easy to scan.</ShadcnBubbleContent>
    </ShadcnBubble>

    <ShadcnBubble Align="ShadcnLogicalAlign.End" Variant="ShadcnBubbleVariant.Default">
        <ShadcnBubbleContent Href="/docs/components/bubble">Sure. Hit me with your best demo.</ShadcnBubbleContent>
    </ShadcnBubble>

    <ShadcnBubble Align="ShadcnLogicalAlign.Start" Variant="ShadcnBubbleVariant.Secondary">
        <ShadcnBubbleContent>Yes. You are reading a demo that is demoing itself. Very meta. Very on-brand.</ShadcnBubbleContent>
        <ShadcnBubbleReactions Side="ShadcnReactionSide.Bottom" Align="ShadcnLogicalAlign.Start" AccessibleName="Reactions for the message">
            <span aria-hidden="true">👍 🔥 👀 <span aria-label="Two more reactions">+2</span></span>
        </ShadcnBubbleReactions>
    </ShadcnBubble>
</ShadcnBubbleGroup>
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

    private const string MessageRazorSource = """
@using Maliev.ShadcnBlazor.Components.Conversation

<ShadcnMessageGroup Class="message-thread">
    <ShadcnMessage Align="ShadcnLogicalAlign.Start">
        <ShadcnMessageAvatar>
            <img src="images/avatars/operator-thai.png" alt="Operator" />
        </ShadcnMessageAvatar>
        <ShadcnMessageContent>
            <ShadcnMessageHeader>Operator</ShadcnMessageHeader>
            <ShadcnBubble Align="ShadcnLogicalAlign.Start" Variant="ShadcnBubbleVariant.Muted">
                <ShadcnBubbleContent>ตรวจสอบไฟล์แล้ว 3 รายการ</ShadcnBubbleContent>
            </ShadcnBubble>
            <ShadcnMessageFooter>
                <button type="button" aria-label="Reply" class="shadcn-message-action">
                    <svg class="shadcn-message-reply-icon" viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                        <path d="m9 7-5 5 5 5M4 12h10a5 5 0 0 1 5 5v1" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" />
                    </svg>
                </button>
            </ShadcnMessageFooter>
        </ShadcnMessageContent>
    </ShadcnMessage>

    <ShadcnMessage Align="ShadcnLogicalAlign.End">
        <ShadcnMessageAvatar>
            <img src="images/avatars/assistant-thai.png" alt="Assistant" />
        </ShadcnMessageAvatar>
        <ShadcnMessageContent>
            <ShadcnMessageHeader>Assistant</ShadcnMessageHeader>
            <ShadcnBubble Align="ShadcnLogicalAlign.End" Variant="ShadcnBubbleVariant.Default">
                <ShadcnBubbleContent>I’ll keep the thread easy to scan.</ShadcnBubbleContent>
            </ShadcnBubble>
            <ShadcnMessageFooter data-visibility="always">
                <span>Sent · 10:42</span>
            </ShadcnMessageFooter>
        </ShadcnMessageContent>
    </ShadcnMessage>
</ShadcnMessageGroup>
""";

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
