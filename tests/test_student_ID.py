import pytest
import requests
import os
import api_info as a
sup_url = a.api_link()


def test_get_student():
  print("test_get_student")
  r = requests.get(a.api_link() + "student/581433")
  assert r.status_code == 200, r.status_code
def test_put_student():
  print("test_put_student")
  r = requests.get(a.api_link() + "student/581433")
  assert r.status_code == 200, r.status_code
