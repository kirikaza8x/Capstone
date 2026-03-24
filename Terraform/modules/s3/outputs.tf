output "bucket_name" {
  value = aws_s3_bucket.media.bucket
}

output "bucket_arn" {
  value = aws_s3_bucket.media.arn
}

output "iam_access_key_id" {
  value = aws_iam_access_key.app.id
}

output "iam_secret_access_key" {
  value     = aws_iam_access_key.app.secret
  sensitive = true
}