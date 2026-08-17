using Maliev.ShadcnBlazor.Components.Content;
using Maliev.ShadcnBlazor.Components.Feedback;
using Maliev.ShadcnBlazor.Components.Feedback.Toast;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;

namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

internal static class FeedbackContentExamples
{
    public static IReadOnlyList<ComponentExampleDefinition> Create(string slug) => slug switch
    {
        "alert" => [Alert()],
        "avatar" => [Avatar()],
        "badge" => [Badge()],
        "card" => [Card()],
        "carousel" => [Carousel()],
        "progress" => [Progress()],
        "skeleton" => [Skeleton()],
        "spinner" => [Spinner()],
        "toast" => [Toast()],
        _ => []
    };

    private static ComponentExampleDefinition Alert()
    {
        var variant = ShadcnAlertVariant.Default; var role = ShadcnAlertRole.Alert; var action = true;
        RenderFragment preview = b =>
        {
            b.OpenComponent<ShadcnAlert>(0);
            b.AddAttribute(1, nameof(ShadcnAlert.Variant), variant);
            b.AddAttribute(2, nameof(ShadcnAlert.AlertRole), role);
            b.AddAttribute(3, "class", "showcase-alert-card");
            b.AddAttribute(4, nameof(ShadcnAlert.ChildContent), (RenderFragment)(content =>
            {
                Add<ShadcnAlertIcon>(content, 0, variant == ShadcnAlertVariant.Destructive ? "!" : "✓");
                Add<ShadcnAlertTitle>(content, 3, variant == ShadcnAlertVariant.Destructive ? "ตรวจสอบข้อมูลก่อนส่ง" : "ชำระเงินสำเร็จ");
                Add<ShadcnAlertDescription>(content, 6, variant == ShadcnAlertVariant.Destructive ? "ยังมีรายการที่ต้องแก้ไขในใบเสนอราคา" : "Payment processed — ใบเสนอราคาพร้อมดำเนินการต่อ");
                if (action)
                {
                    content.OpenComponent<ShadcnAlertAction>(9);
                    content.AddAttribute(10, nameof(ShadcnAlertAction.ChildContent), (RenderFragment)(actionContent =>
                    {
                        actionContent.OpenElement(0, "button");
                        actionContent.AddAttribute(1, "type", "button");
                        actionContent.AddAttribute(2, "class", "showcase-alert-action");
                        actionContent.AddContent(3, variant == ShadcnAlertVariant.Destructive ? "ตรวจสอบ" : "ดูรายละเอียด");
                        actionContent.CloseElement();
                    }));
                    content.CloseComponent();
                }
            }));
            b.CloseComponent();
        };
        const string source = """
<ShadcnAlert Variant="ShadcnAlertVariant.Default" AlertRole="ShadcnAlertRole.Status">
    <ShadcnAlertIcon>✓</ShadcnAlertIcon>
    <ShadcnAlertTitle>ชำระเงินสำเร็จ</ShadcnAlertTitle>
    <ShadcnAlertDescription>
        Payment processed — ใบเสนอราคาพร้อมดำเนินการต่อ
    </ShadcnAlertDescription>
    <ShadcnAlertAction>
        <button type="button">ดูรายละเอียด</button>
    </ShadcnAlertAction>
</ShadcnAlert>
""";
        return Example("alert", "Alert callout", "Compose an accessible shadcn alert with semantic icon, title, description, and an optional action.", source, preview, [EnumSelect("alert-variant", "Variant", variant, v => variant = v), EnumSelect("alert-role", "Role", role, v => role = v), Toggle("alert-action", "Action", v => action = v, true)], ["default", "destructive", "icon", "title", "description", "action", "alert", "status"]);
    }
    private static ComponentExampleDefinition Avatar()
    {
        var size = ShadcnAvatarSize.Default; var failed = false; var badge = false; var group = false;
        RenderFragment preview = b => { b.OpenComponent<AvatarDossierPreview>(0); b.AddAttribute(1, "Size", size); b.AddAttribute(2, "Failed", failed); b.AddAttribute(3, "Badge", badge); b.AddAttribute(4, "Group", group); b.CloseComponent(); };
        string Source() => BuildAvatarSource(size, failed, badge, group);
        var example = Example("avatar", "Avatar gallery", "Compare distinct Thai team portraits, a useful fallback placeholder, presence badge, and stacked group composition in one responsive example.", Source(), preview, [EnumSelect("avatar-size", "Size", size, v => size = v), Toggle("avatar-failed", "Failed image", v => failed = v), Toggle("avatar-badge", "Online badge", v => badge = v), Toggle("avatar-group", "Group", v => group = v)], ["gallery", "sm", "default", "lg", "fallback", "badge", "group", "distinct-images", "responsive"]);
        return example with { RazorSourceProvider = Source };
    }

    private static string BuildAvatarSource(ShadcnAvatarSize size, bool failed, bool badge, bool group)
    {
        var firstSource = failed ? "images/avatars/missing-avatar.png" : "images/avatars/operator-thai.png";
        var badgeMarkup = badge ? "\n            <ShadcnAvatarBadge aria-label=\"Online\" />" : string.Empty;
        var groupMarkup = group
            ? $$"""

<section aria-label="Recently active team members">
    <ShadcnAvatarGroup Size="ShadcnAvatarSize.{{size}}" Overlap="0.625rem">
        <ShadcnAvatar Size="ShadcnAvatarSize.{{size}}">
            <ShadcnAvatarImage Source="{{firstSource}}" Alt="Thai CNC operator" />
            <ShadcnAvatarFallback>NT</ShadcnAvatarFallback>{{badgeMarkup}}
        </ShadcnAvatar>
        <ShadcnAvatar Size="ShadcnAvatarSize.{{size}}">
            <ShadcnAvatarImage Source="images/avatars/reviewer-thai.png" Alt="Thai quality reviewer" />
            <ShadcnAvatarFallback>QR</ShadcnAvatarFallback>
        </ShadcnAvatar>
        <ShadcnAvatar Size="ShadcnAvatarSize.{{size}}">
            <ShadcnAvatarImage Source="images/avatars/assistant-thai.png" Alt="Thai support assistant" />
            <ShadcnAvatarFallback>SA</ShadcnAvatarFallback>
        </ShadcnAvatar>
        <ShadcnAvatarGroupCount Size="ShadcnAvatarSize.{{size}}">+1</ShadcnAvatarGroupCount>
    </ShadcnAvatarGroup>
</section>
"""
            : string.Empty;

        return $$"""
@using Maliev.ShadcnBlazor.Components.Content

<section class="team-avatars" aria-label="Project team avatar examples">
    <article class="team-avatar-profile">
        <ShadcnAvatar Size="ShadcnAvatarSize.{{size}}">
            <ShadcnAvatarImage Source="{{firstSource}}" Alt="Thai CNC operator" />
            <ShadcnAvatarFallback>NT</ShadcnAvatarFallback>{{badgeMarkup}}
        </ShadcnAvatar>
        <span><strong>นที</strong><span>Thai CNC operator</span></span>
    </article>
    <article class="team-avatar-profile">
        <ShadcnAvatar Size="ShadcnAvatarSize.{{size}}">
            <ShadcnAvatarImage Source="images/avatars/reviewer-thai.png" Alt="Thai quality reviewer" />
            <ShadcnAvatarFallback>QR</ShadcnAvatarFallback>
        </ShadcnAvatar>
        <span><strong>พิม</strong><span>Thai quality reviewer</span></span>
    </article>
    <article class="team-avatar-profile">
        <ShadcnAvatar Size="ShadcnAvatarSize.{{size}}">
            <ShadcnAvatarImage Source="images/avatars/coordinator-thai.png" Alt="Thai project coordinator" />
            <ShadcnAvatarFallback>PC</ShadcnAvatarFallback>
        </ShadcnAvatar>
        <span><strong>มาลี</strong><span>Thai project coordinator</span></span>
    </article>
    <article class="team-avatar-profile">
        <ShadcnAvatar Size="ShadcnAvatarSize.{{size}}">
            <ShadcnAvatarFallback>ทีม</ShadcnAvatarFallback>
        </ShadcnAvatar>
        <span><strong>ทีม</strong><span>Fallback placeholder</span></span>
    </article>
</section>{{groupMarkup}}
""";
    }
    private static ComponentExampleDefinition Badge()
    {
        var variant = ShadcnBadgeVariant.Default; var link = false; var invalid = false;
        RenderFragment preview = b =>
        {
            b.OpenComponent<BadgeDossierPreview>(0);
            b.AddAttribute(1, nameof(BadgeDossierPreview.Variant), variant);
            b.AddAttribute(2, nameof(BadgeDossierPreview.Link), link);
            b.AddAttribute(3, nameof(BadgeDossierPreview.Invalid), invalid);
            b.CloseComponent();
        };
        string Source()
        {
            var href = link ? " Href=\"/docs/components/badge\"" : string.Empty;
            var invalidState = invalid ? " aria-invalid=\"true\"" : string.Empty;
            return $$"""
@using Maliev.ShadcnBlazor.Components.Content

<section class="production-queue" aria-labelledby="production-queue-title">
    <header>
        <div>
            <strong id="production-queue-title">Production queue</strong>
            <span>Scan work states without opening each order.</span>
        </div>
        <span>Line 04 · Today</span>
    </header>

    <div class="current-order">
        <span><strong>CNC enclosure</strong><span>Order MO-1842</span></span>
        <ShadcnBadge Variant="ShadcnBadgeVariant.{{variant}}"{{href}}{{invalidState}}>
            <svg data-icon="inline-start" viewBox="0 0 16 16" aria-hidden="true" focusable="false">
                <path d="m3.25 8.2 2.7 2.65 6-6" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" />
            </svg>
            Ready for inspection
        </ShadcnBadge>
    </div>

    <ul aria-label="Badge variants">
        <li><ShadcnBadge Variant="ShadcnBadgeVariant.Default">Approved</ShadcnBadge><span>Released to production</span></li>
        <li><ShadcnBadge Variant="ShadcnBadgeVariant.Secondary">Queued</ShadcnBadge><span>Waiting for a machine</span></li>
        <li><ShadcnBadge Variant="ShadcnBadgeVariant.Destructive">Blocked</ShadcnBadge><span>Requires attention</span></li>
        <li><ShadcnBadge Variant="ShadcnBadgeVariant.Outline">In review</ShadcnBadge><span>Quality check in progress</span></li>
        <li><ShadcnBadge Variant="ShadcnBadgeVariant.Ghost">Draft</ShadcnBadge><span>Not yet scheduled</span></li>
        <li><ShadcnBadge Variant="ShadcnBadgeVariant.Link" Href="/docs/components/badge">View order</ShadcnBadge><span>Linked badge treatment</span></li>
    </ul>
</section>
""";
        }
        var example = Example("badge", "Production status badges", "Compare all six pinned badge treatments in a realistic production queue, then exercise link, focus, and invalid states on the current order.", Source(), preview, [EnumSelect("badge-variant", "Current variant", variant, v => variant = v), Toggle("badge-link", "Current badge is a link", v => link = v), Toggle("badge-invalid", "Invalid state", v => invalid = v)], ["variants", "inline", "icons", "link", "focus", "invalid", "responsive", "rtl"]);
        return example with { RazorSourceProvider = Source };
    }
    private static ComponentExampleDefinition Card()
    {
        var size = ShadcnCardSize.Default; var compactSpacing = false; var action = true;
        RenderFragment preview = b => { b.OpenComponent<CardDossierPreview>(0); b.AddAttribute(1, "Size", size); b.AddAttribute(2, "CompactSpacing", compactSpacing); b.AddAttribute(3, "Action", action); b.CloseComponent(); };
        string Source()
        {
            var sizeMarkup = $"Size=\"ShadcnCardSize.{size}\"";
            var spacingMarkup = compactSpacing ? " Spacing=\"0.75rem\"" : string.Empty;
            var actionMarkup = action
                ? """
        <ShadcnCardAction>
            <ShadcnBadge Variant="ShadcnBadgeVariant.Secondary">Running</ShadcnBadge>
        </ShadcnCardAction>
"""
                : string.Empty;
            var footerActionMarkup = action
                ? """
        <ShadcnButton Variant="ShadcnButtonVariant.Outline" Size="ShadcnButtonSize.Small">View details</ShadcnButton>
"""
                : string.Empty;

            return $"""
<ShadcnCard {sizeMarkup}{spacingMarkup}>
    <ShadcnCardHeader>
        <div>
            <ShadcnCardTitle>Laser cell 04</ShadcnCardTitle>
            <ShadcnCardDescription>CNC milling · Production line A</ShadcnCardDescription>
        </div>
{actionMarkup}    </ShadcnCardHeader>
    <ShadcnCardContent>
        <div>
            <span>Utilization</span>
            <strong>98%</strong>
            <span>Next service</span>
            <strong>22 Aug 2026</strong>
        </div>
        <div>Within target · Updated 2 minutes ago</div>
    </ShadcnCardContent>
    <ShadcnCardFooter>
        <span>Production cell A</span>
{footerActionMarkup}    </ShadcnCardFooter>
</ShadcnCard>
""";
        }

        var source = Source();
        return new ComponentExampleDefinition("card-primary", "Composed card", "Show a production-order card with title, description, action, content, and footer hierarchy.", source, preview, [EnumSelect("card-size", "Size", size, v => size = v), Toggle("card-spacing", "Compact spacing", v => compactSpacing = v), Toggle("card-action", "Action", v => action = v, true)], ["default", "sm", "header", "action", "content", "footer"])
        {
            RazorSourceProvider = Source
        };
    }
    private static ComponentExampleDefinition Carousel()
    {
        var vertical = false; var loop = false; var rtl = false; var reduced = false;
        RenderFragment preview = b =>
        {
            b.OpenComponent<CarouselDossierPreview>(0);
            b.AddAttribute(1, "Vertical", vertical);
            b.AddAttribute(2, "Loop", loop);
            b.AddAttribute(3, "Rtl", rtl);
            b.AddAttribute(4, "ReducedMotion", reduced);
            b.CloseComponent();
        };
        const string source = """
@using Maliev.ShadcnBlazor.Components.Content

<ShadcnCarousel Label="Production queue"
                Orientation="@(vertical ? ShadcnCarouselOrientation.Vertical : ShadcnCarouselOrientation.Horizontal)"
                @bind-SelectedIndex="selectedIndex"
                Options="@Options">
    <ShadcnCarouselContent>
        <ShadcnCarouselItem Index="0" Label="Operations · Laser cell · 98%">
            <article class="queue-slide">Operations · Laser cell · 98% · On schedule</article>
        </ShadcnCarouselItem>
        <ShadcnCarouselItem Index="1" Label="Quality · First-pass yield · 96.2%">
            <article class="queue-slide">Quality · First-pass yield · 96.2% · Target met</article>
        </ShadcnCarouselItem>
        <ShadcnCarouselItem Index="2" Label="Delivery · Orders shipped · 1,284">
            <article class="queue-slide">Delivery · Orders shipped · 1,284 · This month</article>
        </ShadcnCarouselItem>
    </ShadcnCarouselContent>
    <ShadcnCarouselPrevious />
    <ShadcnCarouselNext />
</ShadcnCarousel>

<p aria-live="polite">Slide @(selectedIndex + 1) of 3</p>
<nav aria-label="Choose production queue slide">
    @for (var index = 0; index < 3; index++)
    {
        var slideIndex = index;
        <button type="button"
                aria-label="Show slide @(slideIndex + 1)"
                aria-pressed="@(selectedIndex == slideIndex)"
                @onclick="() => selectedIndex = slideIndex">
            @(slideIndex + 1)
        </button>
    }
</nav>

@code {
    private bool vertical;
    private bool loop;
    private bool rtl;
    private bool reducedMotion;
    private int selectedIndex;
    private ShadcnCarouselOptions Options => new()
    {
        Loop = loop,
        RightToLeft = rtl,
        ReducedMotion = reducedMotion
    };
}
""";
        return Example("carousel", "Carousel engine", "Move through a production queue with arrows, keyboard shortcuts, pointer gestures, slide status, and motion preferences.", source, preview, [Toggle("carousel-vertical", "Vertical", v => vertical = v), Toggle("carousel-loop", "Loop", v => loop = v), Toggle("carousel-rtl", "RTL", v => rtl = v), Toggle("carousel-reduced", "Reduced motion", v => reduced = v)], ["horizontal", "vertical", "loop", "keyboard", "pointer", "status", "dots", "rtl", "reduced-motion"]);
    }
    private static ComponentExampleDefinition Progress()
    {
        var indeterminate = false; var value = 64d; var showValue = true;
        RenderFragment preview = b =>
        {
            b.OpenElement(0, "section");
            b.AddAttribute(1, "class", "showcase-progress-demo");
            b.AddAttribute(2, "aria-label", "Design asset upload");
            b.OpenElement(3, "div");
            b.AddAttribute(4, "class", "showcase-progress-demo__summary");
            b.OpenElement(5, "span");
            b.AddAttribute(6, "aria-hidden", "true");
            b.OpenElement(7, "svg");
            b.AddAttribute(8, "viewBox", "0 0 24 24");
            b.OpenElement(9, "path");
            b.AddAttribute(10, "d", "M12 16V4m0 0-4 4m4-4 4 4M5 20h14");
            b.CloseElement();
            b.CloseElement();
            b.CloseElement();
            b.OpenElement(11, "div");
            b.OpenElement(12, "strong");
            b.AddContent(13, "design-assets.zip");
            b.CloseElement();
            b.OpenElement(14, "small");
            b.AddAttribute(15, "dir", "ltr");
            b.AddContent(16, indeterminate
                ? "Preparing secure upload…"
                : $"{Math.Clamp(value, 0d, 100d) * 28.8d / 100d:0.0} MB of 28.8 MB");
            b.CloseElement();
            b.CloseElement();
            b.CloseElement();
            b.OpenComponent<ShadcnProgress>(17);
            b.AddAttribute(18, "Value", indeterminate ? null : value);
            b.AddAttribute(19, "Label", indeterminate ? "Preparing upload" : "Upload progress");
            b.AddAttribute(20, "ShowValue", showValue);
            b.CloseComponent();
            b.CloseElement();
        };
        string Source() => string.Join(Environment.NewLine,
        [
            "<section class=\"showcase-progress-demo\" aria-label=\"Design asset upload\">",
            "    <div class=\"showcase-progress-demo__summary\">",
            "        <span aria-hidden=\"true\">",
            "            <svg viewBox=\"0 0 24 24\"><path d=\"M12 16V4m0 0-4 4m4-4 4 4M5 20h14\" /></svg>",
            "        </span>",
            "        <div>",
            "            <strong>design-assets.zip</strong>",
            "            <small dir=\"ltr\">@UploadDetail</small>",
            "        </div>",
            "    </div>",
            "    <ShadcnProgress Value=\"@(Indeterminate ? null : Value)\"",
            "                    Label=\"@(Indeterminate ? \"Preparing upload\" : \"Upload progress\")\"",
            "                    ShowValue=\"@ShowValue\" />",
            "</section>",
            string.Empty,
            "@code {",
            $"    private bool Indeterminate = {indeterminate.ToString().ToLowerInvariant()};",
            $"    private double Value = {value:0.##};",
            $"    private bool ShowValue = {showValue.ToString().ToLowerInvariant()};",
            "    private string UploadDetail => Indeterminate",
            "        ? \"Preparing secure upload…\"",
            "        : $\"{Math.Clamp(Value, 0d, 100d) * 28.8d / 100d:0.0} MB of 28.8 MB\";",
            "}"
        ]);
        var source = Source();
        return new ComponentExampleDefinition("progress-primary", "Progress", "Compare determinate and indeterminate accessible progress.", source, preview, [Toggle("progress-indeterminate", "Indeterminate", v => indeterminate = v), Number("progress-value", "Value", value, v => value = v), Toggle("progress-show-value", "Show value", v => showValue = v, true)], ["determinate", "indeterminate", "label", "value"])
        {
            RazorSourceProvider = Source
        };
    }
    private static ComponentExampleDefinition Skeleton()
    {
        var circle = false; var motion = true;
        RenderFragment preview = b => { b.OpenComponent<SkeletonDossierPreview>(0); b.AddAttribute(1, "Circle", circle); b.AddAttribute(2, "Motion", motion); b.CloseComponent(); };
        const string source = """
<section class="showcase-skeleton-layout" aria-label="Loading production overview">
    <div class="showcase-skeleton-layout__header">
        <ShadcnSkeleton Shape="ShadcnSkeletonShape.Circle" Animation="ShadcnSkeletonAnimation.Pulse" Style="width:2.75rem;height:2.75rem" />
        <div><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:12rem;height:1rem" /><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:8rem;height:.75rem" /></div>
    </div>
    <div class="showcase-skeleton-layout__body"><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:68%;height:1rem" /><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:92%;height:1rem" /><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:48%;height:1rem" /></div>
    <div class="showcase-skeleton-layout__cards"><article><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:100%;height:4.5rem" /><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:70%;height:.75rem" /></article><article><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:100%;height:4.5rem" /><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:70%;height:.75rem" /></article><article><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:100%;height:4.5rem" /><ShadcnSkeleton Animation="ShadcnSkeletonAnimation.Pulse" Style="width:70%;height:.75rem" /></article></div>
</section>
""";
        return Example("skeleton", "Skeleton", "Preview a realistic page-loading layout with shared geometry and respectful motion.", source, preview, [Toggle("skeleton-circle", "Avatar circle", v => circle = v), Toggle("skeleton-motion", "Pulse", v => motion = v, true)], ["layout", "circle", "pulse", "reduced-motion"]);
    }
    private static ComponentExampleDefinition Spinner()
    {
        var decorative = false; var large = false;
        RenderFragment preview = b =>
        {
            b.OpenElement(0, "div"); b.AddAttribute(1, "class", "showcase-spinner-task");
            b.OpenComponent<ShadcnSpinner>(2); b.AddAttribute(3, "Label", decorative ? null : "กำลังประมวลผลการชำระเงิน"); b.AddAttribute(4, "Size", large ? "1.5rem" : "1rem"); b.CloseComponent();
            b.OpenElement(5, "span"); b.AddContent(6, "Processing payment…"); b.CloseElement(); b.OpenElement(7, "strong"); b.AddContent(8, "฿100.00"); b.CloseElement(); b.CloseElement();
        };
        const string source = """
<div class="payment-status">
    <ShadcnSpinner Label="กำลังประมวลผลการชำระเงิน" />
    <span>Processing payment…</span>
    <strong>฿100.00</strong>
</div>
""";
        return Example("spinner", "Spinner", "Show loading context with an announced spinner, decorative option, and payment amount.", source, preview, [Toggle("spinner-decorative", "Decorative", v => decorative = v), Toggle("spinner-large", "Large", v => large = v)], ["status", "decorative", "size", "reduced-motion"]);
    }
    private static ComponentExampleDefinition Toast()
    {
        var limit = 3d; var start = false; var reduced = false; var type = ShadcnToastType.Success; var priority = ShadcnToastPriority.Normal;
        RenderFragment preview = b => { b.OpenComponent<ToastDossierPreview>(0); b.AddAttribute(1, "MaximumVisible", (int)limit); b.AddAttribute(2, "Placement", start ? ShadcnToastPlacement.BottomStart : ShadcnToastPlacement.BottomEnd); b.AddAttribute(3, "ReducedMotion", reduced); b.AddAttribute(4, "Type", type); b.AddAttribute(5, "Priority", priority); b.CloseComponent(); };
        string Source() => string.Join(Environment.NewLine,
        [
            "@inject IShadcnToastService Toasts",
            string.Empty,
            "<ShadcnButton OnClick=\"Show\">Show localized toast</ShadcnButton>",
            $"<ShadcnToaster MaximumVisible=\"{Math.Max(1, (int)limit)}\" Placement=\"ShadcnToastPlacement.{(start ? ShadcnToastPlacement.BottomStart : ShadcnToastPlacement.BottomEnd)}\" ReducedMotion=\"{reduced.ToString().ToLowerInvariant()}\" />",
            string.Empty,
            "@code {",
            "    private void Show()",
            "    {",
            $"        Toasts.Show(new(\"บันทึกแล้ว — Saved\", Type: ShadcnToastType.{type}, ActionLabel: \"เลิกทำ\", Action: () => Task.CompletedTask, Priority: ShadcnToastPriority.{priority}));",
            "    }",
            "}",
            string.Empty
        ]);
        var source = Source();
        return new ComponentExampleDefinition("toast-primary", "Toast viewport", "Trigger the real page-level toast queue; notifications stack outside this preview and remain keyboard reachable.", source, preview, [Number("toast-limit", "Visible limit", limit, v => limit = Math.Max(1, v)), Toggle("toast-start", "Logical start", v => start = v), Toggle("toast-reduced", "Reduced motion", v => reduced = v), EnumSelect("toast-type", "Type", type, v => type = v), EnumSelect("toast-priority", "Priority", priority, v => priority = v)], ["queue", "limit", "promise", "pause", "swipe", "F6", "rtl", "reduced-motion", "priority"])
        {
            RazorSourceProvider = Source
        };
    }

    private static ComponentExampleDefinition Example(string slug, string title, string description, string source, RenderFragment preview, IReadOnlyList<ComponentParameterControl> controls, IReadOnlyList<string> tags) => new($"{slug}-primary", title, description, source, preview, controls, tags);
    private static ComponentParameterControl Toggle(string id, string label, Action<bool> apply, bool initial = false) => new(id, label, ComponentParameterControlKind.Toggle, initial.ToString(), [], value => apply(bool.Parse(value)));
    private static ComponentParameterControl Number(string id, string label, double value, Action<double> apply) => new(id, label, ComponentParameterControlKind.Number, value.ToString(System.Globalization.CultureInfo.InvariantCulture), [], text => apply(double.Parse(text, System.Globalization.CultureInfo.InvariantCulture)));
    private static ComponentParameterControl EnumSelect<T>(string id, string label, T value, Action<T> apply) where T : struct, Enum => new(id, label, ComponentParameterControlKind.Select, value.ToString(), Enum.GetNames<T>(), text => apply(Enum.Parse<T>(text)));
    private static RenderFragment Text(string value) => b => b.AddContent(0, value);
    private static void Add<T>(RenderTreeBuilder b, int sequence, string text) where T : IComponent { b.OpenComponent<T>(sequence); b.AddAttribute(sequence + 1, "ChildContent", Text(text)); b.CloseComponent(); }
}
