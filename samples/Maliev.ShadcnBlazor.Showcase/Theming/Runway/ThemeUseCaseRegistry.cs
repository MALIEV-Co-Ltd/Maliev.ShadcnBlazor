namespace Maliev.ShadcnBlazor.Showcase.Theming.Runway;

public sealed class ThemeUseCaseRegistry : IThemeUseCaseRegistry
{
    public IReadOnlyList<ThemeUseCaseDefinition> All { get; } =
    [
        Use("production-capacity", 1, ThemeBentoSize.Standard, "Production capacity", "กำลังการผลิต", "ShadcnProgress", "ShadcnBadge", "ShadcnButton"),
        Use("production-analytics", 2, ThemeBentoSize.Wide, "Production analytics chart", "กราฟวิเคราะห์การผลิต", "ShadcnChart"),
        Use("operator-profile", 3, ThemeBentoSize.Standard, "Operator profile", "โปรไฟล์ผู้ปฏิบัติงาน", "ShadcnAvatar", "ShadcnInput", "ShadcnButton"),
        Use("inspection-alerts", 4, ThemeBentoSize.Standard, "Inspection alerts", "การแจ้งเตือนตรวจสอบ", "ShadcnCheckbox", "ShadcnSwitch", "ShadcnButton"),
        Use("quotation-files", 5, ThemeBentoSize.Wide, "Quotation dropzone", "พื้นที่อัปโหลดใบเสนอราคา", "ShadcnDropzone", "ShadcnProgress", "ShadcnBadge"),
        Use("inspection-table", 6, ThemeBentoSize.Wide, "Inspection results table", "ตารางผลการตรวจสอบ", "ShadcnTable"),
        Use("shipping-handoff", 7, ThemeBentoSize.Standard, "Shipping handoff", "การส่งมอบงาน", "ShadcnInput", "ShadcnSelect", "ShadcnCheckbox"),
        Use("quotation-data-table", 8, ThemeBentoSize.Wide, "Quotation data table", "ตารางข้อมูลใบเสนอราคา", "ShadcnDataTable"),
        Use("drawing-attachment", 9, ThemeBentoSize.Wide, "Drawing attachment", "ไฟล์แนบแบบงาน", "ShadcnAttachment", "ShadcnAttachmentAction"),
        Use("quality-alert", 10, ThemeBentoSize.Wide, "Quality alert", "การแจ้งเตือนคุณภาพ", "ShadcnAlert"),
        Use("deposit-approval", 11, ThemeBentoSize.Standard, "Deposit approval", "อนุมัติเงินมัดจำ", "ShadcnBadge", "ShadcnSeparator", "ShadcnButton"),
        Use("conversation-marker", 12, ThemeBentoSize.Wide, "Conversation marker", "ตัวคั่นบทสนทนา", "ShadcnMarker"),
        Use("assigned-reviewers", 13, ThemeBentoSize.Standard, "Assigned reviewers", "ผู้ตรวจสอบที่ได้รับมอบหมาย", "ShadcnAvatarGroup", "ShadcnBadge", "ShadcnButton"),
        Use("assistant-conversation", 14, ThemeBentoSize.Wide, "Message scroller conversation", "บทสนทนาแบบเลื่อนข้อความ", "ShadcnMessageScroller", "ShadcnMessage", "ShadcnBubble", "ShadcnAvatar", "ShadcnButton"),
        Use("project-questionnaire", 15, ThemeBentoSize.Wide, "Interactive questionnaire", "แบบสอบถามเชิงโต้ตอบ", "ShadcnQuestionnaire", "ShadcnQuestionnaireChoice", "ShadcnQuestionnaireInput"),
        Use("work-order-navigation", 16, ThemeBentoSize.Wide, "Work-order navigation", "การนำทางใบสั่งงาน", "ShadcnBreadcrumb", "ShadcnItem", "ShadcnBadge"),
        Use("machine-cell", 17, ThemeBentoSize.Standard, "Machine cell", "สถานีเครื่องจักร", "ShadcnCard", "ShadcnProgress", "ShadcnBadge"),
        Use("issue-report", 18, ThemeBentoSize.Standard, "Issue report", "รายงานปัญหา", "ShadcnInput", "ShadcnTextarea", "ShadcnAlert"),
        Use("dispatch-confirmation", 19, ThemeBentoSize.Standard, "Dispatch confirmation", "ยืนยันการจัดส่ง", "ShadcnAlertDialog", "ShadcnBadge", "ShadcnButton"),
        Use("quotation-actions", 20, ThemeBentoSize.Standard, "Quotation actions", "การดำเนินการใบเสนอราคา", "ShadcnDropdownMenu"),
        Use("reviewer-details", 21, ThemeBentoSize.Standard, "Reviewer details", "รายละเอียดผู้ตรวจสอบ", "ShadcnHoverCard", "ShadcnTooltip", "ShadcnAvatar"),
        Use("file-context", 22, ThemeBentoSize.Wide, "Drawing review workspace", "พื้นที่ตรวจสอบแบบ", "ShadcnContextMenu", "ShadcnAttachment", "ShadcnAvatar", "ShadcnBadge", "ShadcnButton"),
        Use("contact-dialog", 23, ThemeBentoSize.Standard, "Production contact", "ผู้ติดต่อฝ่ายผลิต", "ShadcnDialog", "ShadcnInput"),
        Use("dispatch-drawer", 24, ThemeBentoSize.Standard, "Dispatch review", "ตรวจสอบการจัดส่ง", "ShadcnDrawer"),
        Use("delivery-sheet", 25, ThemeBentoSize.Standard, "Delivery schedule", "กำหนดการส่งมอบ", "ShadcnSheet"),
        Use("tooltip-guidance", 26, ThemeBentoSize.Standard, "Inspection guidance", "คำแนะนำการตรวจสอบ", "ShadcnTooltip", "ShadcnButton"),
        Suite("production-planning-suite", 27, "Production planning console", "ศูนย์วางแผนการผลิต"),
        Suite("quality-release-suite", 28, "Quality release console", "ศูนย์อนุมัติคุณภาพ"),
        Suite("customer-handoff-suite", 29, "Customer handoff console", "ศูนย์ส่งมอบลูกค้า"),
        Use("quality-trend", 30, ThemeBentoSize.Wide, "Quality trend", "แนวโน้มคุณภาพ", "ShadcnChart", "ShadcnBadge"),
        Use("order-mix", 31, ThemeBentoSize.Standard, "Order mix", "สัดส่วนคำสั่งซื้อ", "ShadcnChart", "ShadcnBadge"),
        Use("api-credentials", 32, ThemeBentoSize.Wide, "Integration credentials", "ข้อมูลรับรองการเชื่อมต่อ", "ShadcnSecretInput", "ShadcnButton", "ShadcnBadge"),
        Use("machine-password", 33, ThemeBentoSize.Standard, "Machine access", "การเข้าถึงเครื่องจักร", "ShadcnSecretInput", "ShadcnButton", "ShadcnAlert"),
        Use("webhook-secret", 34, ThemeBentoSize.Standard, "Webhook signing", "การลงนามเว็บฮุก", "ShadcnSecretInput", "ShadcnButton", "ShadcnBadge"),
        Use("drawing-preview", 35, ThemeBentoSize.Wide, "Drawing comparison", "เปรียบเทียบแบบงาน", "ShadcnAspectRatio", "ShadcnBadge", "ShadcnButton"),
        Use("inspection-camera", 36, ThemeBentoSize.Standard, "Inspection camera", "กล้องตรวจสอบ", "ShadcnAspectRatio", "ShadcnBadge", "ShadcnButton"),
        Use("customer-proof", 37, ThemeBentoSize.Standard, "Customer proof", "หลักฐานสำหรับลูกค้า", "ShadcnAspectRatio", "ShadcnBadge", "ShadcnButton")
    ];

    private static ThemeUseCaseDefinition Use(string id, int order, ThemeBentoSize size, string en, string th, params string[] components) =>
        new(id, order, size, en, th, Array.AsReadOnly(components));

    private static ThemeUseCaseDefinition Suite(string id, int order, string en, string th) =>
        Use(id, order, ThemeBentoSize.Wide, en, th,
            "ShadcnAccordion", "ShadcnAlert", "ShadcnAlertDialog", "ShadcnAspectRatio", "ShadcnAttachment", "ShadcnAvatar",
            "ShadcnBadge", "ShadcnBentoGrid", "ShadcnBreadcrumb", "ShadcnBubble", "ShadcnButton", "ShadcnButtonGroup",
            "ShadcnCalendar", "ShadcnCard", "ShadcnCarousel", "ShadcnChart", "ShadcnCheckbox", "ShadcnCodeBlock",
            "ShadcnCollapsible", "ShadcnCombobox", "ShadcnCommand", "ShadcnContextMenu", "ShadcnDataTable", "ShadcnDatePicker",
            "ShadcnDialog", "ShadcnDirectionProvider", "ShadcnDrawer", "ShadcnDropdownMenu", "ShadcnDropzone", "ShadcnEmpty",
            "ShadcnField", "ShadcnHoverCard", "ShadcnInput", "ShadcnInputGroup", "ShadcnInputOtp", "ShadcnItem", "ShadcnKbd",
            "ShadcnLabel", "ShadcnMarker", "ShadcnMenubar", "ShadcnMessage", "ShadcnMessageScroller", "ShadcnNativeSelect",
            "ShadcnNavigationMenu", "ShadcnPagination", "ShadcnPopover", "ShadcnProgress", "ShadcnQuestionnaire", "ShadcnRadioGroup",
            "ShadcnResizableGroup", "ShadcnScrollArea", "ShadcnSelect", "ShadcnSeparator", "ShadcnSheet", "ShadcnSidebar", "ShadcnSkeleton",
            "ShadcnSlider", "ShadcnSpinner", "ShadcnSwitch", "ShadcnTable", "ShadcnTabs", "ShadcnTextarea", "ShadcnToaster",
            "ShadcnToggle", "ShadcnToggleGroup", "ShadcnTooltip", "ShadcnTypography", "ShadcnVisualStyleScope");
}

