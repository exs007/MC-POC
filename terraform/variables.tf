variable "environment" {
  type = string
}

variable "location" {
  type = string
}

variable "prefix" {
  type    = string
  default = "at-dev-ms"
}

variable "acr_name" {
  type = string
}

variable "tags" {
  type = map(any)
}

variable "sql_server_admin" {
  type = string
}

variable "sql_server_pass" {
  type = string
}

variable "host_api" {
  type = string
}

variable "host_logs" {
  type = string
}