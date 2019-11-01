import pytest
import requests
import os
import api_info as a

def test_post_coachTutorant():
  tut = a.tutorantId()
  a.delete_coachTutorant(tut)
  url = a.api_link() + "coachTutorant/tutorant/" + tut
  payload = "{\n\"studentIDTutorant\":\""+ tut +"\",\n\"studentIDCoach\": \""+ a.coachId() +"\",\n\"status\":\"Pending\"\n}"
  headers = {
    'Content-Type': "application/json",
    'cache-control': "no-cache"}
  r = requests.post(url, data=payload, headers=headers)
  if r.status_code is not 201:
    a.create_coachTutorant(tut)
  assert r.status_code == 201, r.status_code
def test_get_coachTutorants_coach_byId():
  r = requests.get(a.api_link() + "coachTutorant/coach/" + a.coachId())
  assert r.status_code == 200, r.status_code
def test_delete_coachTutorant_coach_byId():
  r = requests.delete(a.api_link() + "coachTutorant/coach/" + a.coachId())
  if r.status_code is not 204:
    a.create_multiple_coachTutorant()
  assert r.status_code == 204, r.status_code
  a.create_multiple_coachTutorant()
def test_get_coachTutorants_tutorant_byId():
  r = requests.get(a.api_link() + "coachTutorant/tutorant/" + a.tutorantId())
  assert r.status_code == 200, r.status_code
def test_put_coachTutorant_tutorant_byId():
  tut = a.tutorantId()
  url = a.api_link() + "coachTutorant"
  payload = "{\n\"studentIDTutorant\":\""+ tut +"\",\n\"studentIDCoach\": \""+ a.coachId() +"\",\n\"status\":\"Completed\"\n}"
  headers = {
    'Content-Type': "application/json",
    'cache-control': "no-cache"}
  r = requests.put(url, data=payload, headers=headers)
  assert r.status_code == 204, r.status_code
def test_delete_coachTutorant_tutorant_byId():
  r = requests.delete(a.api_link() + "coachTutorant/tutorant/" + a.tutorantId())
  if r.status_code is not 204:
    a.create_coachTutorant(a.tutorantId())
  assert r.status_code == 204, r.status_code
  a.create_coachTutorant(a.tutorantId())