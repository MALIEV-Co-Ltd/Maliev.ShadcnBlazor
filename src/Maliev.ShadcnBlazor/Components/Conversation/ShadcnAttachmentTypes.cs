namespace Maliev.ShadcnBlazor.Components.Conversation;

public enum ShadcnAttachmentState { Idle, Uploading, Processing, Error, Done }
public enum ShadcnAttachmentSize { Default, Small, ExtraSmall }
public enum ShadcnAttachmentOrientation { Horizontal, Vertical }
public enum ShadcnAttachmentMediaVariant { Icon, Image }
public enum ShadcnAttachmentActionKind { None, Remove, Retry, Cancel, Download, Custom }
