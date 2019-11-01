import pytest
import requests
import os
import api_info as a
from cerberus import Validator


def test_get_tutorants():
  r = requests.get(a.api_link() + "profile/tutorant")
  assert r.status_code == 200, r.status_code
  v = Validator(a.s_tutorant())
  assert v.validate(r.json()[0]['tutorant']) == True, v.errors
  v = Validator(a.s_studentData())
  assert v.validate(r.json()[0]['user']) == True, v.errors
def test_post_tutorant():
  a.delete_tutorant()
  url = a.api_link() + "profile/tutorant"
  payload = "{\n   \"user\": {\n      \"studentID\": \""+ a.tutorantId() + "\",\n      \"firstName\": \"TestTutorant\",\n      \"surName\": \"Test\",\n      \"phoneNumber\": \"0692495724\",\n      \"interests\": \"Programming (C only), Servers, Cisco\",\n      \"photo\": \"https://i.imgur.com/Tl5sYD6.jpg\",\n      \"description\": \"I am a student\",\n      \"degree\": \"HBO\",\n      \"study\": \"Technische Informatica\",\n      \"studyYear\": 4\n    }, \n  \"tutorant\": {\n      \"studentID\": \""+ a.tutorantId() + "\"\n } \n}"  
  headers = {
    'Content-Type': "application/json",
    'cache-control': "no-cache"}
  r = requests.request("POST", url, data=payload, headers=headers)
  if r.status_code is not 201:
    a.create_tutorant()
  assert r.status_code == 201, r.status_code
def test_get_tutorant_byId():
  r = requests.get(a.api_link() + "profile/tutorant/" + a.tutorantId())
  assert r.status_code == 200, r.status_code
def test_delete_tutorant_byId():
  r = requests.delete(a.api_link() + "profile/tutorant/" + a.tutorantId())
  assert r.status_code == 204, r.status_code
  a.create_tutorant(a.tutorantId())