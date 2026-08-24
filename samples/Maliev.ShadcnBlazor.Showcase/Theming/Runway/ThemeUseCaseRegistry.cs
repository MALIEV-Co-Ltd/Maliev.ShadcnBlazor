namespace Maliev.ShadcnBlazor.Showcase.Theming.Runway;

public sealed class ThemeUseCaseRegistry : IThemeUseCaseRegistry
{
    public IReadOnlyList<ThemeUseCaseDefinition> All { get; } =
    [
        Use("production-capacity", 1, ThemeBentoSize.Standard, "Production capacity", "กำลังการผลิต", "ShadcnProgress", "ShadcnBadge", "ShadcnButton"),
        Use("production-analytics", 2, ThemeBentoSize.Wide, "Production analytics chart", "กราฟวิเคราะห์การผลิต", "ShadcnChart"),
        Use("operator-profile", 3, ThemeBentoSize.Standard, "Operator profile", "โปรไฟล์ผู้ปฏิบัติงาน", "ShadcnAvatar", "ShadcnInput", "ShadcnButton"),
        Use("drawing-attachment", 4, ThemeBentoSize.Standard, "Drawing attachment", "ไฟล์แนบแบบงาน", "ShadcnAttachment", "ShadcnAttachmentAction"),
        Use("quotation-files", 5, ThemeBentoSize.Wide, "Quotation dropzone", "พื้นที่อัปโหลดใบเสนอราคา", "ShadcnDropzone", "ShadcnProgress", "ShadcnBadge"),
        Use("inspection-table", 6, ThemeBentoSize.Wide, "Inspection results table", "ตารางผลการตรวจสอบ", "ShadcnTable"),
        Use("shipping-handoff", 7, ThemeBentoSize.Standard, "Shipping handoff", "การส่งมอบงาน", "ShadcnInput", "ShadcnSelect", "ShadcnCheckbox"),
        Use("quotation-data-table", 8, ThemeBentoSize.Wide, "Quotation data table", "ตารางข้อมูลใบเสนอราคา", "ShadcnDataTable"),
        Use("inspection-alerts", 9, ThemeBentoSize.Standard, "Inspection alerts", "การแจ้งเตือนตรวจสอบ", "ShadcnCheckbox", "ShadcnSwitch", "ShadcnButton"),
        Use("quality-alert", 10, ThemeBentoSize.Standard, "Quality alert", "การแจ้งเตือนคุณภาพ", "ShadcnAlert"),
        Use("deposit-approval", 11, ThemeBentoSize.Standard, "Deposit approval", "อนุมัติเงินมัดจำ", "ShadcnBadge", "ShadcnSeparator", "ShadcnButton"),
        Use("conversation-marker", 12, ThemeBentoSize.Standard, "Conversation marker", "ตัวคั่นบทสนทนา", "ShadcnMarker"),
        Use("assigned-reviewers", 13, ThemeBentoSize.Standard, "Assigned reviewers", "ผู้ตรวจสอบที่ได้รับมอบหมาย", "ShadcnAvatarGroup", "ShadcnBadge", "ShadcnButton"),
        Use("assistant-conversation", 14, ThemeBentoSize.Wide, "Message scroller conversation", "บทสนทนาแบบเลื่อนข้อความ", "ShadcnMessageScroller", "ShadcnMessage", "ShadcnBubble", "ShadcnAvatar", "ShadcnButton"),
        Use("project-questionnaire", 15, ThemeBentoSize.Standard, "Interactive questionnaire", "แบบสอบถามเชิงโต้ตอบ", "ShadcnQuestionnaire", "ShadcnQuestionnaireChoice", "ShadcnQuestionnaireInput"),
        Use("work-order-navigation", 16, ThemeBentoSize.Wide, "Work-order navigation", "การนำทางใบสั่งงาน", "ShadcnBreadcrumb", "ShadcnItem", "ShadcnBadge"),
        Use("machine-cell", 17, ThemeBentoSize.Standard, "Machine cell", "สถานีเครื่องจักร", "ShadcnCard", "ShadcnProgress", "ShadcnBadge"),
        Use("issue-report", 18, ThemeBentoSize.Standard, "Issue report", "รายงานปัญหา", "ShadcnInput", "ShadcnTextarea", "ShadcnAlert"),
        Use("dispatch-confirmation", 19, ThemeBentoSize.Standard, "Dispatch confirmation", "ยืนยันการจัดส่ง", "ShadcnAlertDialog", "ShadcnBadge", "ShadcnButton"),
        Use("quotation-actions", 20, ThemeBentoSize.Standard, "Quotation actions", "การดำเนินการใบเสนอราคา", "ShadcnDropdownMenu"),
        Use("reviewer-details", 21, ThemeBentoSize.Standard, "Reviewer details", "รายละเอียดผู้ตรวจสอบ", "ShadcnHoverCard", "ShadcnTooltip", "ShadcnAvatar"),
        Use("file-context", 22, ThemeBentoSize.Standard, "Drawing workspace", "พื้นที่ทำงานแบบ", "ShadcnContextMenu"),
        Use("contact-dialog", 23, ThemeBentoSize.Standard, "Production contact", "ผู้ติดต่อฝ่ายผลิต", "ShadcnDialog", "ShadcnInput"),
        Use("dispatch-drawer", 24, ThemeBentoSize.Standard, "Dispatch review", "ตรวจสอบการจัดส่ง", "ShadcnDrawer"),
        Use("delivery-sheet", 25, ThemeBentoSize.Standard, "Delivery schedule", "กำหนดการส่งมอบ", "ShadcnSheet"),
        Use("tooltip-guidance", 26, ThemeBentoSize.Standard, "Inspection guidance", "คำแนะนำการตรวจสอบ", "ShadcnTooltip", "ShadcnButton")
    ];

    private static ThemeUseCaseDefinition Use(string id, int order, ThemeBentoSize size, string en, string th, params string[] components) =>
        new(id, order, size, en, th, Array.AsReadOnly(components));
}

