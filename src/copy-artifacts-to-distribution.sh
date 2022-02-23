#!/bin/bash

echo "list in directory /distribution:"
ls --color=auto /distribution || true
echo ""

echo "list /app directory:"
ls --color=auto ./app/ || true
echo ""

echo "copy files and directories to /distribution:"
cp -a ./app/* /distribution/
echo ""

echo "Sleep 15 second. Wait work script for copy artifacts to distribution"
sleep 15
echo ""

echo "list in directory /distribution:"
ls --color=auto /distribution || true
echo ""