{{- define "ticket-management.fullname" -}}
{{ .Release.Name }}
{{- end -}}

{{- define "ticket-management.labels" -}}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end -}}
