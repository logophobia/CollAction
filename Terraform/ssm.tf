variable "securestring_parameters" {
  type = set(string)
  default = [
    "AdminEmail",
    "AdminPassword",
    "ApplicationInsights/InstrumentationKey",
    "Authentication/Facebook/AppId",
    "Authentication/Facebook/AppSecret",
    "Authentication/Google/ClientId",
    "Authentication/Google/ClientSecret",
    "Authentication/Twitter/ConsumerKey",
    "Authentication/Twitter/ConsumerSecret",
    "DbUser",
    "DbPassword",
    "DisqusSite",
    "DisqusSiteId",
    "MailChimpAccount",
    "MailChimpUserId",
    "MailChimpKey",
    "MailChimpNewsletterListId",
    "S3AwsAccessKey",
    "S3AwsAccessKeyID",
    "SesAwsAccessKey",
    "SesAwsAccessKeyId",
    "SlackHook",
    "StripeChargeableWebhookSecret",
    "StripePaymentEventWebhookSecret",
    "StripePublicApiKey",
    "StripeSecretApiKey",
    "GoogleAnalyticsID",
    "FacebookPixelID"
  ]
}

variable "string_parameters" {
  type = set(string)
  default = [
    "APPINSIGHTS_JAVASCRIPT_ENABLED",
    "ASPNETCORE_ENVIRONMENT",
    "Db",
    "DbHost",
    "DbPort",
    "FromAddress",
    "ImportLocationData",
    "MailChimpServer",
    "PublicAddress",
    "ResetTestDatabase",
    "S3Bucket",
    "S3Region",
    "SeedTestData",
    "SesRegion"
  ]
}

resource "aws_ssm_parameter" "securestring_parameters" {
  for_each = var.securestring_parameters

  name = "/collaction/${var.Environment}/${each.key}"
  type = "SecureString"
  value = ""
}

resource "aws_ssm_parameter" "string_parameters" {
  for_each = var.string_parameters

  name = "/collaction/${var.Environment}/${each.key}"
  type = "String"
  value = ""
}
