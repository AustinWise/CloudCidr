#!/bin/sh

set -ex

cd -P -- "$(dirname -- "$0")"

# https://support.google.com/a/answer/10026322?hl=en
curl https://www.gstatic.com/ipranges/cloud.json -o data/google-cloud.json
# https://docs.aws.amazon.com/vpc/latest/userguide/aws-ip-ranges.html
curl https://ip-ranges.amazonaws.com/ip-ranges.json -o data/aws.json
# Seriously?
curl -sf 'https://www.microsoft.com/en-us/download/details.aspx?id=56519' | grep -Eo 'href="https[^"]+\.json\"' | head -n 1 | sed -e 's/href="//' -e 's/"$//' | xargs curl -o data/azure.json
