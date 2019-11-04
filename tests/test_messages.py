import pytest
import requests
import os
import api_info as a

def test_get_messages_coachId_tutorantId():
  params = {
    "amountOfMessages": 20
  }
  r = requests.get(a.api_link() + "messages/" + a.coachId() + "/" + a.tutorantId(), params=params)
  assert r.status_code == 200, r.status_code
def test_get_message_byId():
  r = requests.get(a.api_link() + "message/1")
  assert r.status_code == 200, r.status_code
#def test_delete_message_byId():
#not possible to test fully, as the create does not give back message ID.
#  r = requests.delete(a.api_link() + "message/" + a.messageId())
#  assert r.status_code == 204, r.status_code
def test_post_message():
  payload = "{\n\t\"type\": \"text\",\n\t\"payload\": \"Hi Barend ;)\",\n\t\"created\": \"2019-11-1\",\n\t\"lastModified\": \"2019-11-1\",\n\t\"senderID\": \"701\",\n\t\"receiverID\": \"710\"\n}"
  headers = {
    'Content-Type': "application/json",
    'cache-control': "no-cache"}
  r = requests.post(a.api_link() + "message/", payload, headers)
  assert r.status_code == 201, r.status_code